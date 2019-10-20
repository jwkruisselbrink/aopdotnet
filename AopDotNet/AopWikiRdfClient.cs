using AopDotNet.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace AopDotNet {
    public class AopWikiRdfClient {

        public Uri Endpoint { private set; get; }

        public AopWikiRdfClient(Uri endpoint) {
            Endpoint = endpoint;
        }

        public List<AopNetwork> GetAllAopNetworks() {
            return GetAopNetworks();
        }

        public AopNetwork GetAopNetwork(string idAopNetwork) {
            var network = GetAopNetworks(idAopNetwork).First();
            network.KeyEvents = GetKeyEvents(idAopNetwork);
            network.KeyEventRelationships = GetKeyEventRelationShips(idAopNetwork, network.KeyEvents.ToDictionary(r => r.Id));
            return network;
        }

        private List<AopNetwork> GetAopNetworks(string id = null) {
            var queryString = new SparqlParameterizedString();
            queryString.Namespaces.AddNamespace("dc", new Uri("http://purl.org/dc/elements/1.1/"));
            queryString.Namespaces.AddNamespace("aopo", new Uri("http://aopkb.org/aop_ontology#"));
            queryString.Namespaces.AddNamespace("rdfs", new Uri("http://www.w3.org/2000/01/rdf-schema#"));
            if (!string.IsNullOrEmpty(id)) {
                queryString.SetUri("id", new Uri($@"http://identifiers.org/aop/{id}"));
                queryString.CommandText = (@"
                    SELECT *
                    WHERE {
                     ?Aop a aopo:AdverseOutcomePathway ;
                     dc:title ?AopName ; 
                     rdfs:label ?AopId .
                     FILTER (?Aop = @id)
                    }
                ");
            } else {
                queryString.CommandText = (@"
                    SELECT *
                    WHERE {
                     ?Aop a aopo:AdverseOutcomePathway ;
                     dc:title ?AopName ; 
                     rdfs:label ?AopId .
                    }
                ");
            }
            queryString.SetUri("value", new Uri("http://aopwiki-rdf.prod.openrisknet.org/sparql/"));
            var parser = new SparqlQueryParser();
            var processor = new RemoteQueryProcessor(new SparqlRemoteEndpoint(Endpoint));
            var query = parser.ParseFromString(queryString);
            var resultSet = processor.ProcessQuery(query);
            if (resultSet is SparqlResultSet) {
                var result = new List<AopNetwork>();
                foreach (var resultRow in (resultSet as SparqlResultSet)) {
                    var recordId = ((ILiteralNode)resultRow["AopId"]).Value;
                    var record = new AopNetwork() {
                        Id = recordId.Replace("AOP ", ""),
                        Name = recordId,
                        Description = ((ILiteralNode)resultRow["AopName"]).Value,
                    };
                    result.Add(record);
                }
                return result;
            } else {
                return null;
            }
        }

        private List<KeyEvent> GetKeyEvents(string idAopNetwork) {
            var endpoint = new Uri("http://aopwiki-rdf.prod.openrisknet.org/sparql/");
            var queryString = new SparqlParameterizedString();
            queryString.Namespaces.AddNamespace("dc", new Uri("http://purl.org/dc/elements/1.1/"));
            queryString.Namespaces.AddNamespace("aopo", new Uri("http://aopkb.org/aop_ontology#"));
            queryString.Namespaces.AddNamespace("rdfs", new Uri("http://www.w3.org/2000/01/rdf-schema#"));
            queryString.SetUri("value", new Uri("http://aopwiki-rdf.prod.openrisknet.org/sparql/"));
            queryString.SetUri("id", new Uri($@"http://identifiers.org/aop/{idAopNetwork}"));
            queryString.CommandText = (@"
                SELECT ?AopName ?KEid ?KEname ?AopAo ?AopMie ?CellTypeContext ?OrganContext
                WHERE {
                    ?KE a aopo:KeyEvent ; 
                     rdfs:label ?KEid ; 
                     dc:title ?KEname .
                    ?AopId a aopo:AdverseOutcomePathway ;
                     aopo:has_key_event ?KE ;
                     rdfs:label ?AopName .
                    OPTIONAL {
                        ?AopAo aopo:has_adverse_outcome ?KE ;
                         aopo:AdverseOutcomePathway ?AopId .
                    }
                    OPTIONAL {
                        ?AopMie aopo:has_molecular_initiating_event ?KE ;
                        aopo:AdverseOutcomePathway ?AopId .
                    }
                    OPTIONAL { ?KE aopo:CellTypeContext ?CellTypeContext . }
                    OPTIONAL { ?KE aopo:OrganContext ?OrganContext . }
                    FILTER (?AopId = @id)
                }");
            var parser = new SparqlQueryParser();
            var processor = new RemoteQueryProcessor(new SparqlRemoteEndpoint(endpoint));
            var query = parser.ParseFromString(queryString);
            var resultSet = processor.ProcessQuery(query);
            if (resultSet is SparqlResultSet) {
                var result = new List<KeyEvent>();
                foreach (var resultRow in (resultSet as SparqlResultSet)) {
                    var id = (resultRow["KEid"] as ILiteralNode)?.Value;
                    var isMie = (resultRow["AopMie"] as IUriNode) != null;
                    var isAo = (resultRow["AopAo"] as IUriNode) != null;
                    var hasCellTypeContext = (resultRow["CellTypeContext"] as IUriNode) != null;
                    var hasOrganContext = (resultRow["OrganContext"] as IUriNode) != null;
                    var record = new KeyEvent() {
                        Id = id.Replace("KE ", ""),
                        Name = id,
                        Description = ((ILiteralNode)resultRow["KEname"]).Value,
                        BiologicalOrganisation = GetBiologicalOrganisation(isMie, isAo, hasCellTypeContext, hasOrganContext)
                    };
                    result.Add(record);
                }
                return result;
            } else {
                return null;
            }
        }

        private List<KeyEventRelationship> GetKeyEventRelationShips(
            string idAopNetwork,
            IDictionary<string, KeyEvent> keyEvents
        ) {
            var endpoint = new Uri("http://aopwiki-rdf.prod.openrisknet.org/sparql/");
            var queryString = new SparqlParameterizedString();
            queryString.Namespaces.AddNamespace("dc", new Uri("http://purl.org/dc/elements/1.1/"));
            queryString.Namespaces.AddNamespace("aopo", new Uri("http://aopkb.org/aop_ontology#"));
            queryString.Namespaces.AddNamespace("rdfs", new Uri("http://www.w3.org/2000/01/rdf-schema#"));
            queryString.SetUri("value", new Uri("http://aopwiki-rdf.prod.openrisknet.org/sparql/"));
            queryString.SetUri("id", new Uri($@"http://identifiers.org/aop/{idAopNetwork}"));
            queryString.CommandText = (@"
                SELECT ?KeUpstreamName ?KeDownstreamName
                WHERE {
                    ?KeDownstream a aopo:KeyEvent ;
                        rdfs:label ?KeDownstreamName .
                    ?KeUpstream a aopo:KeyEvent ;
                        rdfs:label ?KeUpstreamName .
                    ?Ker a aopo:KeyEventRelationship ;
                        aopo:has_upstream_key_event ?KeUpstream ;
                        aopo:has_downstream_key_event ?KeDownstream .
                    ?Aop a aopo:AdverseOutcomePathway ;
                        dc:identifier ?AopId ;
                        rdfs:label ?AopName ;
                        aopo:has_key_event ?KeUpstream ;
                        aopo:has_key_event ?KeDownstream ;
                        aopo:has_key_event_relationship ?Ker .
                    FILTER (?AopId = @id)
                }");
            var parser = new SparqlQueryParser();
            var processor = new RemoteQueryProcessor(new SparqlRemoteEndpoint(endpoint));
            var query = parser.ParseFromString(queryString);
            var resultSet = processor.ProcessQuery(query);
            if (resultSet is SparqlResultSet) {
                var result = new List<KeyEventRelationship>();
                foreach (var resultRow in (resultSet as SparqlResultSet)) {
                    var idDownstreamKeyEvent = ((ILiteralNode)resultRow["KeDownstreamName"]).Value.Replace("KE ", "");
                    var idUpstreamKeyEvent = ((ILiteralNode)resultRow["KeUpstreamName"]).Value.Replace("KE ", "");
                    keyEvents.TryGetValue(idDownstreamKeyEvent, out var downstreamKeyEvent);
                    keyEvents.TryGetValue(idUpstreamKeyEvent, out var upstreamKeyEvent);
                    var record = new KeyEventRelationship() {
                        FromNode = upstreamKeyEvent,
                        ToNode = downstreamKeyEvent,
                    };
                    result.Add(record);
                }
                return result;
            } else {
                return null;
            }
        }

        private BiologicalOrganisation GetBiologicalOrganisation(
            bool isMie,
            bool isAo,
            bool hasCellTypeContext,
            bool hasOrganContext
        ) {
            if (isMie) {
                return BiologicalOrganisation.Molecular;
            }
            return BiologicalOrganisation.Unknown;
        }
    }
}
