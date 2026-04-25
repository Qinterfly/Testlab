using LMSTestLabAutomation;
using System.Collections.Generic;

namespace Core
{
    static class Constants
    {
        // Project
        public const int kMaxAttemptAccess = 5000;
        public const int kDepthSearch = 100;
        public const char kPathDelimiter = '/';

        // Data
        public const int kNumDirections = 3;
    }

    public class Manager
    {
        public Manager()
        {
            clear();
        }

        // Initialize the application
        public bool initialize()
        {
            try
            {
                mApp = new Application();
                if (mApp.Name == "")
                    mApp.Init("-w DesktopStandard");
                mDatabase = mApp.ActiveBook.Database();
                mUnitSystem = mApp.UnitSystem;
                mGeometry = (IGeometry)mDatabase.GetItem("Geometry");
            }
            catch
            {
                clear();
                return false;
            }
            return true;
        }

        // Drop all the references
        public void clear()
        {
            mApp = null;
            mDatabase = null;
            mUnitSystem = null;
            mGeometry = null;
        }

        // Initialize the application and open a project
        public bool openProject(in string pathFile)
        {
            try
            {
                if (mApp == null)
                    mApp = new Application();
                if (mApp.Name == "")
                    mApp.Init("-w DesktopStandard " + pathFile);
                else
                    mApp.OpenProject(pathFile);
                mDatabase = mApp.ActiveBook.Database();
                mUnitSystem = mApp.UnitSystem;
                mGeometry = (IGeometry)mDatabase.GetItem("Geometry");
                mLastPath = pathFile;
            }
            catch
            {
                clear();
                return false;
            }
            return true;
        }

        // Check if the application is initialized
        public bool isValid()
        {
            return mApp != null;
        }

        // Retrieve path to a project
        public string getPath()
        {
            return mLastPath;
        }

        // Get a name of active section
        public string getActiveSection()
        {
            try
            {
                return mApp.ActiveBook.ActiveSectionName;
            }
            catch
            {
                return string.Empty;
            }
        }

        // Create a section
        public void createSection(string section, bool isSelect)
        {
            int numAttempts = Constants.kMaxAttemptAccess;
            try
            {
                while (!isSectionExist(section) && --numAttempts > 0)
                    mApp.ActiveBook.NewSection(section);
                if (isSelect)
                    mApp.ActiveBook.SwitchSection(section);
            }
            catch
            {

            }
        }

        // Check whether a section exists or not
        public bool isSectionExist(string section)
        {
            if (mDatabase == null)
                return false;
            try
            {
                AttributeMap map = mDatabase.SectionNames;
                foreach (string tSection in map)
                {
                    if (tSection.Equals(section))
                        return true;
                }
            }
            catch
            {

            }
            return false;
        }

        // Create a folder
        public void createFolder(string section, string folder)
        {
            int numAttempts = Constants.kMaxAttemptAccess;
            try
            {
                while (!isFolderExist(section, folder) && --numAttempts > 0)
                    mDatabase.AddFolder(section, folder);
            }
            catch
            {

            }
        }

        // Check whether a folder exists or not
        public bool isFolderExist(string section, string folder)
        {
            AttributeMap map = mDatabase.ElementNames[section, Constants.kDepthSearch].KeyNames;
            foreach (string tFolder in map)
            {
                if (tFolder.Equals(folder))
                    return true;
            }
            return false;
        }

        // Retrieve responses using their paths inside project
        public List<Response> getResponses(List<string> listPathSignals)
        {
            List<Response> responses = new List<Response>();
            try
            {
                foreach (string pathSignal in listPathSignals)
                {
                    IBlock2 signal = (IBlock2)mDatabase.GetItem(pathSignal);
                    if (signal != null)
                    {
                        string path = fixPath(pathSignal);
                        Response response = acquireResponse(path, signal, signal.Properties);
                        if (response != null)
                            responses.Add(response);
                    }
                }
            }
            catch
            {

            }
            return responses;
        }

        // Retrieve responses selected via Navigator
        public List<Response> getSelectedResponses()
        {
            List<Response> responses = new List<Response>();
            try
            {
                if (mApp == null)
                    return responses;
                DataWatch dataWatch = mApp.ActiveBook.FindDataWatch("Navigator_SelectedOIDs");
                IData dataSelected = dataWatch.Data;
                if (dataSelected == null)
                    return responses;
                AttributeMap selectedMap = dataSelected.AttributeMap;
                int numSelected = selectedMap.Count;
                for (int iSelected = 0; iSelected != numSelected; ++iSelected)
                {
                    DataWatch blockWatch = mApp.FindDataWatch(selectedMap[iSelected]);
                    if (blockWatch.Data.Type != "LmsHq::DataModelI::Expression::CBufferIBlock")
                        continue;

                    // Retrieving path
                    IData dataOID = selectedMap[iSelected].AttributeMap["OID"];
                    string pathSignal = dataOID.AttributeMap["Path"].AttributeMap["PathString"];

                    // Retreiving signals
                    IBlock2 signal = blockWatch.Data;
                    if (signal != null)
                    {
                        string path = fixPath(pathSignal);
                        Response response = acquireResponse(path, signal, signal.Properties);
                        if (response != null)
                            responses.Add(response);
                    }
                }
            }
            catch
            {

            }
            return responses;
        }

        // Add responses to specified path
        public bool addResponses(in List<Response> responses, in string path)
        {
            int numResponses = responses.Count;
            try
            {
                for (int iResponse = 0; iResponse != numResponses; ++iResponse)
                {
                    Response response = responses[iResponse];

                    // Create data block
                    IBlock2 block;
                    if (response.isComplex())
                        block = createComplexBlock(response);
                    else
                        block = createRealBlock(response);

                    // Add it to the database
                    string name = response.Header.Name;
                    if (name.Length == 0)
                        continue;
                    mDatabase.AddItem(path, name, block, null, Constants.kMaxAttemptAccess);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        // Get the current geometry
        public Geometry getGeometry()
        {
            return new Geometry(mGeometry);
        }

        // Construct a response using block data
        private Response acquireResponse(in string path, in IBlock2 signal, in AttributeMap props)
        {
            const int kFRFType = 12;

            ResponseType type = ResponseType.kNone;
            string measuredQuantity = props["Measured quantity"];

            // Determine the response type
            // 15A version
            if (measuredQuantity != null)
            {
                if (measuredQuantity.Equals("Acceleration"))
                    type = ResponseType.kAccel;
                else if (measuredQuantity.Equals("Force"))
                    type = ResponseType.kForce;
            }
            // 12A version
            else
            {
                IQuantity quantityY = props["Y axis unit"];
                string unitY = mUnitSystem.Label(quantityY);
                if (unitY.Equals("g"))
                    type = ResponseType.kAccel;
                if (unitY.Equals("N"))
                    type = ResponseType.kForce;
            }
            if (type == ResponseType.kNone)
                return null;

            // Checking the reference point in case of FRF                        
            int iFunctionClass = props["Function class"].AttributeMap["EnumValue"];
            bool isFRF = iFunctionClass == kFRFType;

            // Get the keys and values
            double[] keys = (double[])signal.XValues;
            double[,] data = (double[,])signal.YValues;
            if (keys.Length <= 1)
                return null;

            // Split the value fields
            int length = data.GetLength(0);
            double[] realValues = new double[length];
            double[] imagValues = new double[length];
            for (int k = 0; k != length; ++k)
            {
                realValues[k] = data[k, 0];
                imagValues[k] = data[k, 1];
            }

            // Set the response
            Response response = new Response();
            // Data
            response.Keys = keys;
            response.RealValues = realValues;
            response.ImagValues = imagValues;
            // General
            ResponseHeader header = response.Header;
            header.Type = type;
            header.Path = path;
            header.OriginalRun = props["Original run"].AttributeMap["Contents"];
            header.Name = signal.Label;
            header.Channel = props["Channel id"];
            header.NumAverages = props["Number of averages"];
            header.Dimension = props["Transducer id"];
            header.Transducer = props["Transducer sn"];
            header.FunctionClass = isFRF ? "FRF" : "Spectrum";
            header.Comment = props["User comment"];
            // Point
            ResponsePoint point = header.Point;
            point.Name = props["Point id"];
            point.Node = props["Point id node"];
            point.Component = props["Point id component"];
            point.Direction = getDirectionValue(props["Point direction absolute"]);
            point.Sign = getSignValue(props["Point direction sign"]);
            // Reference point
            ResponsePoint refPoint = header.RefPoint;
            refPoint.Name = props["Reference point id"];
            refPoint.Node = props["Reference point id node"];
            refPoint.Component = props["Reference point id component"];
            refPoint.Direction = getDirectionValue(props["Reference point direction absolute"]);
            refPoint.Sign = getSignValue(props["Reference point direction sign"]);
            // Unit
            if (type == ResponseType.kAccel)
                header.Unit = isFRF ? new ResponseUnit(0, -1, 0, 1.0, "(m/s^2)/N") : new ResponseUnit(1, 0, -2, 1.0, "m/s^2");
            else
                header.Unit = isFRF ? new ResponseUnit(0, 0, 0, 1.0, "/") : new ResponseUnit(1, 1, -2, 1.0, "N");

            return response;
        }

        // Create IBlock2 consisted of complex data
        private IBlock2 createComplexBlock(in Response response)
        {
            // Create the block
            int numKeys = response.Keys.Length;
            AttributeMap map = mApp.CreateAttributeMap();
            map.Add("BlockSize", numKeys);
            IBlock2 block = (IBlock2)mApp.CreateObject("LmsHq::DataModelI::Expression::CBufferIBlock", map);

            // Set the quantities
            block = block.ReplaceXQuantity(mUnitSystem.QuantityFrequency);
            block = block.ReplaceYQuantity(mUnitSystem.QuantityAcceleration);

            // Set the data
            int numValues = response.RealValues.Length;
            double[,] values = new double[numValues, 2];
            for (int i = 0; i != numValues; ++i)
            {
                values[i, 0] = response.RealValues[i];
                values[i, 1] = response.ImagValues[i];
            }
            block = block.ReplaceXDoubleValues(response.Keys);
            block = block.ReplaceYComplexValues(values);

            // Set the header
            setBlockHeader(ref block, response.Header);

            return block;
        }

        // Create IBlock2 data consisted of real data
        private IBlock2 createRealBlock(in Response response)
        {
            // Create the block
            int numKeys = response.Keys.Length;
            AttributeMap map = mApp.CreateAttributeMap();
            map.Add("BlockSize", numKeys);
            IBlock2 block = (IBlock2)mApp.CreateObject("LmsHq::DataModelI::Expression::CBufferIBlock", map);

            // Set the quantities
            block = block.ReplaceXQuantity(mUnitSystem.QuantityTime);
            block = block.ReplaceYQuantity(mUnitSystem.QuantityAcceleration);

            // Set the data
            block = block.ReplaceXDoubleValues(response.Keys);
            block = block.ReplaceYDoubleValues(response.RealValues);

            // Set the header
            setBlockHeader(ref block, response.Header);

            return block;
        }

        // Set the block attributes and header
        private void setBlockHeader(ref IBlock2 block, in ResponseHeader rHeader)
        {
            // Set the attributes
            AttributeMap attributes = block.UserAttributes;
            attributes.Add("Channel id", rHeader.Channel);
            attributes.Add("Channelgroup", "Measure");
            attributes.Add("Transducer id", rHeader.Dimension);
            attributes.Add("Transducer sn", rHeader.Transducer);
            attributes.Add("User comment", rHeader.Comment);
            block = block.ReplaceUserAttributes(attributes);

            // Set the block header
            IHeader bHeader = block.Header;
            string pointSign = getSignLabel(rHeader.Point.Sign);
            string pointName = $"{rHeader.Point.Component}:{rHeader.Point.Node}";
            string pointDirection = pointSign + getDirectionLabel(rHeader.Point.Direction);
            bHeader = bHeader.Edit("Point id", pointName);
            bHeader = bHeader.Edit("Point direction", createDirection(pointDirection));
            if (rHeader.RefPoint.Node != string.Empty)
            {
                string refPointSign = getSignLabel(rHeader.RefPoint.Sign);
                string refPointName = $"{rHeader.RefPoint.Component}:{rHeader.RefPoint.Node}";
                string refPointDirection = refPointSign + getDirectionLabel(rHeader.RefPoint.Direction);
                bHeader = bHeader.Edit("Reference point id", refPointName);
                bHeader = bHeader.Edit("Reference point direction", createDirection(refPointDirection));
            }
            block = block.ReplaceHeader(bHeader);
        }

        // Create IData consisted of point direction data
        private IData createDirection(in string label)
        {
            var kMapLabels = new Dictionary<string, int>
            {
                { "+X", 1 }, { "-X", 2 },
                { "+Y", 3 }, { "-Y", 4 },
                { "+Z", 5 }, { "-Z", 6 },
            };
            AttributeMap map = mApp.CreateAttributeMap();
            map.Add("EnumValue", kMapLabels.ContainsKey(label) ? kMapLabels[label] : 0);
            IData direction = mApp.CreateObject("LmsHq::DataModelI::Channel::CBufferIEnumDirections", map);
            return direction;
        }

        // Remove special characters from the path
        private string fixPath(in string path)
        {
            string result = path;
            if (result.Contains("\\"))
            {
                result = result.Replace("/", "");
                result = result.Replace("\\", "/");
            }
            if (!result.EndsWith("/"))
                result += "/";
            return result;
        }

        // Retrieve direction value
        private Direction getDirectionValue(string label)
        {
            switch (label)
            {
                case "X":
                    return Direction.kX;
                case "Y":
                    return Direction.kY;
                case "Z":
                    return Direction.kZ;
            }
            return Direction.kNone;
        }

        // Retrieve direction label
        private string getDirectionLabel(Direction value)
        {
            switch (value)
            {
                case Direction.kX:
                    return "X";
                case Direction.kY:
                    return "Y";
                case Direction.kZ:
                    return "Z";
            }
            return string.Empty;
        }

        // Retrieve sign value
        private int getSignValue(string label)
        {
            return label == "-" ? -1 : +1;
        }

        // Retrieve sign label
        private string getSignLabel(int value)
        {
            return value < 0 ? "-" : "+";
        }

        private IApplication mApp;
        private IDatabase mDatabase;
        private IUnitSystem mUnitSystem;
        private IGeometry mGeometry;
        private string mLastPath;
    }
}
