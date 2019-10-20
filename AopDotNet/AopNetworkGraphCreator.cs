using AopDotNet.Entities;
using Svg;
using Svg.DataTypes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace AopDotNet {
    public class AopNetworkGraphCreator {

        public double BlockHeight { get; set; } = 30;
        public double BlockWidth { get; set; } = 125;
        public double VerticalMargin { get; set; } = 20;
        public double HorizontalMargin { get; set; } = 40;

        private readonly SvgMarker arrowMarker = CreateArrowMarker();

        public AopNetworkGraphCreator() { }

        /// <summary>
        /// Creates an svg of the AOP network graph and writes it to the specified file.
        /// </summary>
        /// <param name="aopNetwork"></param>
        /// <param name="fileName"></param>
        public void Create(AopNetwork aopNetwork, string fileName) {
            var layers = aopNetwork.GetAopNetworkLayers();

            var title = !string.IsNullOrEmpty(aopNetwork.Name) ? aopNetwork.Name : null;
            var showTitle = title != null;
            var offsetX = 10D;
            var offsetY = showTitle ? 40D : 20D;

            var width = offsetX + layers.Count * (BlockWidth + HorizontalMargin) - HorizontalMargin;
            var height = offsetY + layers.Max(r => r.KeyEvents.Count) * (BlockHeight + VerticalMargin) - VerticalMargin;
            var doc = new SvgDocument() {
                Width = (float)(width),
                Height = (float)(height),
                FontSize = 10,
                FontFamily = "Arial",
            };
            var defsElement = new SvgDefinitionList() { ID = "defsMap" };
            doc.Children.Add(defsElement);
            defsElement.Children.Add(arrowMarker);

            if (showTitle) {
                var text = new SvgText() {
                    FontSize = 14,
                    FontWeight = SvgFontWeight.Bold,
                    Nodes = { new SvgContentNode() { Content = title } },
                    TextAnchor = SvgTextAnchor.Middle,
                    X = new SvgUnitCollection() { 0f },
                    Y = new SvgUnitCollection() { 0f },
                    Dx = new SvgUnitCollection() { (float)width / 2f },
                    Fill = new SvgColourServer(Color.Black),
                };
                text.Dy = new SvgUnitCollection() { 2 * text.Bounds.Height };
                doc.Children.Add(text);
            }

            var keyEventNodes = DrawKeyEvents(
                doc,
                layers,
                offsetX,
                offsetY
            );
            var indirectKers = aopNetwork.GetIndirectKeyEventRelationships();
            var cyclicKers = aopNetwork.FindFeedbackRelationships();
            var kers = aopNetwork.KeyEventRelationships
                .Except(indirectKers)
                .Except(cyclicKers)
                .ToList();
            DrawKeyEventRelationships(doc, kers, keyEventNodes);
            doc.FlushStyles(true);
            doc.Write(fileName);
        }

        /// <summary>
        /// Draws the key event blocks and returns a dictionary in which the rectangle
        /// of the drawn block can be found for each key event can be found.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="layers"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <returns></returns>
        private Dictionary<KeyEvent, SvgRectangle> DrawKeyEvents(
            SvgDocument doc,
            IList<AopNetworkLayer> layers,
            double offsetX,
            double offsetY
        ) {
            var nodeRects = new Dictionary<KeyEvent, SvgRectangle>();
            var maxLayer = layers.Max(r => r.KeyEvents.Count);
            var x = offsetX;
            var previousBiologicalOrganisation = layers[0].BiologicalOrganisation();
            for (int i = 0; i < layers.Count; i++) {
                var layer = layers[i];
                if (i > 0) {
                    var line = new SvgLine() {
                        StartX = (float)(x - HorizontalMargin / 2),
                        StartY = (float)offsetY,
                        EndX = (float)(x - HorizontalMargin / 2),
                        EndY = (float)(offsetY + maxLayer * (BlockHeight + VerticalMargin) - VerticalMargin),
                        Stroke = new SvgColourServer(Color.Black),
                        StrokeDashArray = new SvgUnitCollection() { 4f },
                    };
                    doc.Children.Add(line);
                }

                var y = offsetY + (maxLayer - layer.KeyEvents.Count) * (BlockHeight + VerticalMargin) / 2;

                var keyEvents = layer.KeyEvents.OrderBy(r => r.Id).ToList();
                for (int j = 0; j < keyEvents.Count; j++) {
                    var keyEvent = keyEvents[j];
                    var group = new SvgGroup();
                    var rect = new SvgRectangle() {
                        X = new SvgUnit((float)x),
                        Y = new SvgUnit((float)y),
                        Width = new SvgUnit((float)BlockWidth),
                        Height = new SvgUnit((float)BlockHeight),
                        Stroke = new SvgColourServer(Color.Black),
                        Fill = new SvgColourServer(GetKeyEventBlockColor(keyEvent.BiologicalOrganisation)),
                        FillOpacity = 0.7f,
                        CornerRadiusX = 5f,
                        CornerRadiusY = 5f,
                    };
                    group.Children.Add(rect);
                    nodeRects.Add(keyEvent, rect);

                    var text = new SvgText() {
                        Nodes = { new SvgContentNode() { Content = keyEvent.Name } },
                        TextAnchor = SvgTextAnchor.Middle,
                        X = new SvgUnitCollection() { (float)x },
                        Y = new SvgUnitCollection() { (float)y },
                        Dx = new SvgUnitCollection() { (float)BlockWidth / 2f },
                        Fill = new SvgColourServer(Color.Black),
                    };
                    text.Dy = new SvgUnitCollection() { (float)BlockHeight / 2f + text.Bounds.Height / 4f };
                    group.Children.Add(text);

                    doc.Children.Add(group);

                    previousBiologicalOrganisation = layer.BiologicalOrganisation();
                    y += BlockHeight + VerticalMargin;
                }
                x += BlockWidth + HorizontalMargin;
            }
            return nodeRects;
        }

        /// <summary>
        /// Creates the key-event relationship connectors.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="kers"></param>
        /// <param name="keyEventNodes"></param>
        private void DrawKeyEventRelationships(SvgDocument doc, ICollection<KeyEventRelationship> kers, Dictionary<KeyEvent, SvgRectangle> keyEventNodes) {
            foreach (var ker in kers) {
                var startNode = keyEventNodes[ker.FromNode];
                var endNode = keyEventNodes[ker.ToNode];
                var line = new SvgLine() {
                    StartX = startNode.X + startNode.Width,
                    StartY = startNode.Y + startNode.Height / 2,
                    EndX = endNode.X,
                    EndY = endNode.Y + startNode.Height / 2,
                    Stroke = new SvgColourServer(Color.Black),
                    MarkerEnd = new Uri(string.Format("url(#{0})", arrowMarker.ID), UriKind.Relative)
                };
                doc.Children.Add(line);
            }
        }

        private Color GetKeyEventBlockColor(BiologicalOrganisation biologicalOrganisation) {
            switch (biologicalOrganisation) {
                case BiologicalOrganisation.Molecular:
                    return Color.Green;
                case BiologicalOrganisation.Cellular:
                    return Color.GreenYellow;
                case BiologicalOrganisation.Organelle:
                    return Color.Orange;
                case BiologicalOrganisation.Individual:
                    return Color.Red;
                case BiologicalOrganisation.Population:
                    return Color.Red;
                default:
                    return Color.LightGray;
            }
        }

        private static SvgMarker CreateArrowMarker() {
            return new SvgMarker() {
                ID = "markerArrow",
                RefX = 10f,
                RefY = 5f,
                MarkerUnits = SvgMarkerUnits.StrokeWidth,
                MarkerWidth = 10,
                MarkerHeight = 10,
                Orient = new SvgOrient() { IsAuto = true },
                Children = {
                    new SvgPath() {
                        ID = "pathMarkerArrow",
                        Fill = new SvgColourServer(Color.Black),
                        PathData = SvgPathBuilder.Parse(@"M 0 0 L 10 5 L 0 10 z")
                    }
                }
            };
        }
    }
}
