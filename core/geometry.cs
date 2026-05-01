using LMSTestLabAutomation;
using System;
using System.Collections.Generic;

namespace Core
{
    public class Geometry
    {
        public Geometry(IGeometry geometry)
        {
            initialize(geometry);
        }

        private void initialize(IGeometry geometry)
        {
            if (geometry == null)
                return;

            // Initialize the temporary variables
            Array X, Y, Z, rotXY, rotXZ, rotYZ;
            Array nodeNamesA, nodeNamesB, nodeNamesC, nodeNamesD;
            Array slaveNodeNames, masterNodeNames1, masterNodeNames2, masterNodeNames3, masterNodeNames4;

            // Loop through all the components
            Array componentNames = geometry.ComponentNames;
            int numComponents = componentNames.Length;
            Components = new List<Component>(numComponents);
            for (int iComponent = 0; iComponent != numComponents; ++iComponent)
            {
                string componentName = (string)componentNames.GetValue(iComponent);

                // Get the nodes
                Array nodeNames = geometry.ComponentNodeNames[componentName];
                int numNodes = nodeNames.Length;
                geometry.ComponentNodesValues(componentName, nodeNames, out X, out Y, out Z, out rotXY, out rotXZ, out rotYZ, LocalCoordinates: 0);
                List<Node> nodes = new List<Node>(numNodes);
                Dictionary<string, int> mapNodes = new Dictionary<string, int>();
                for (int iNode = 0; iNode != numNodes; ++iNode)
                {
                    Node node = new Node();
                    nodes.Add(node);
                    // Name
                    node.Name = (string)nodeNames.GetValue(iNode);
                    mapNodes.Add(node.Name, iNode);
                    // Coordinates
                    node.Coordinates = new double[Constants.kNumDirections];
                    node.Coordinates[0] = (double)X.GetValue(iNode);
                    node.Coordinates[1] = (double)Y.GetValue(iNode);
                    node.Coordinates[2] = (double)Z.GetValue(iNode);
                    // Angles
                    node.Angles = new double[Constants.kNumDirections];
                    node.Angles[0] = (double)rotXY.GetValue(iNode);
                    node.Angles[1] = -(double)rotXZ.GetValue(iNode); // (!) -rotY
                    node.Angles[2] = (double)rotYZ.GetValue(iNode);
                }

                // Get the lines
                geometry.ComponentLines(componentName, out nodeNamesA, out nodeNamesB);
                int numLines = nodeNamesA.Length;
                int[,] lines = new int[numLines, 2];
                for (int iLine = 0; iLine != numLines; ++iLine)
                {
                    lines[iLine, 0] = mapNodes[(string)nodeNamesA.GetValue(iLine)];
                    lines[iLine, 1] = mapNodes[(string)nodeNamesB.GetValue(iLine)];
                }

                // Get the triangles
                geometry.ComponentTrias(componentName, out nodeNamesA, out nodeNamesB, out nodeNamesC);
                int numTrias = nodeNamesA.Length;
                int[,] trias = new int[numTrias, 3];
                for (int iTri = 0; iTri != numTrias; ++iTri)
                {
                    trias[iTri, 0] = mapNodes[(string)nodeNamesA.GetValue(iTri)];
                    trias[iTri, 1] = mapNodes[(string)nodeNamesB.GetValue(iTri)];
                    trias[iTri, 2] = mapNodes[(string)nodeNamesC.GetValue(iTri)];
                }

                // Get the quads
                geometry.ComponentQuads(componentName, out nodeNamesA, out nodeNamesB, out nodeNamesC, out nodeNamesD);
                int numQuads = nodeNamesA.Length;
                int[,] quads = new int[numQuads, 4];
                for (int iQuad = 0; iQuad != numQuads; ++iQuad)
                {
                    quads[iQuad, 0] = mapNodes[(string)nodeNamesA.GetValue(iQuad)];
                    quads[iQuad, 1] = mapNodes[(string)nodeNamesB.GetValue(iQuad)];
                    quads[iQuad, 2] = mapNodes[(string)nodeNamesC.GetValue(iQuad)];
                    quads[iQuad, 3] = mapNodes[(string)nodeNamesD.GetValue(iQuad)];
                }

                // Get the component coordinates and angles
                geometry.ComponentValues(componentName, out double tX, out double tY, out double tZ, out double tRotXY, out double tRotXZ, out double tRotYZ);
                double[] componentCoordinates = new double[3] { tX, tY, tZ };
                double[] componentAngles = new double[3] { tRotXY, -tRotXZ, tRotYZ }; // (!) -rotY

                // Set the component
                Component component = new Component();
                Components.Add(component);
                component.Name = componentName;
                component.Coordinates = componentCoordinates;
                component.Angles = componentAngles;
                component.Nodes = nodes;
                component.Lines = lines;
                component.Trias = trias;
                component.Quads = quads;
            }

            // Slaves
            geometry.Slaves(out slaveNodeNames, out masterNodeNames1, out masterNodeNames2, out masterNodeNames3, out masterNodeNames4, out X, out Y, out Z);
            int numSlaves = masterNodeNames1.Length;
            Dependencies = new List<Dependency>(numSlaves);
            for (int iSlave = 0; iSlave != numSlaves; ++iSlave)
            {
                Dependency dependency = new Dependency();

                // Dependent node
                dependency.Slave = (string)slaveNodeNames.GetValue(iSlave);

                // Master nodes
                dependency.Masters = new string[4];
                dependency.Masters[0] = (string)masterNodeNames1.GetValue(iSlave);
                dependency.Masters[1] = (string)masterNodeNames2.GetValue(iSlave);
                dependency.Masters[2] = (string)masterNodeNames3.GetValue(iSlave);
                dependency.Masters[3] = (string)masterNodeNames4.GetValue(iSlave);

                // Directional flags
                dependency.Flags = new int[3];
                dependency.Flags[0] = (int)X.GetValue(iSlave);
                dependency.Flags[1] = (int)Y.GetValue(iSlave);
                dependency.Flags[2] = (int)Z.GetValue(iSlave);

                Dependencies.Add(dependency);
            }
        }

        public List<Component> Components;
        public List<Dependency> Dependencies;
    }

    public class Node
    {
        public string Name;
        public double[] Coordinates;
        public double[] Angles;
    }

    public class Component
    {
        public string Name;
        public double[] Coordinates;
        public double[] Angles;
        public List<Node> Nodes;
        public int[,] Lines;
        public int[,] Trias;
        public int[,] Quads;
    }

    public class Dependency
    {
        public string Slave;
        public string[] Masters;
        public int[] Flags;
    }
}
