using LMSTestLabAutomation;
using System.Collections.Generic;

namespace Core
{
    static class Constants
    {
        public const int kMaxAttemptAccess = 5000;
        public const int kDepthSearch = 100;
        public const char kPathDelimiter = '/';
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
        public string path()
        {
            return mLastPath;
        }

        // Create a section
        public void createSection(string section, bool isSelect)
        {
            int numAttempts = Constants.kMaxAttemptAccess;
            while (!isSectionExist(section) && --numAttempts > 0)
                mApp.ActiveBook.NewSection(section);
            if (isSelect)
                mApp.ActiveBook.SwitchSection(section);
        }

        // Check whether a section exists or not
        public bool isSectionExist(string section)
        {
            if (mDatabase == null)
                return false;
            AttributeMap map = mDatabase.SectionNames;
            foreach (string tSection in map)
            {
                if (tSection.Equals(section))
                    return true;
            }
            return false;
        }

        // Create a folder
        public void createFolder(string section, string folder)
        {
            int numAttempts = Constants.kMaxAttemptAccess;
            while (!isFolderExist(section, folder) && --numAttempts > 0)
                mDatabase.AddFolder(section, folder);
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
                        Response response = acquireResponse(pathSignal, signal, signal.Properties);
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
                        Response response = acquireResponse(pathSignal, signal, signal.Properties);
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
                for (int i = 0; i != numResponses; ++i)
                {
                    Response response = responses[i];
                    if (response.RealValues.Length == response.ImagValues.Length)
                    {
                        IBlock2 block = createSpectrumBlock(response);
                        mDatabase.AddItem(path, response.Name, block, null, Constants.kMaxAttemptAccess);
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        // Construct a response using block data
        private Response acquireResponse(in string path, in IBlock2 signal, in AttributeMap props)
        {
            ResponseType type = ResponseType.kUnknown;
            string measuredQuantity = props["Measured quantity"];
            // Determination of the type of the signal
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
            if (type == ResponseType.kUnknown)
                return null;
            double[] keys = (double[])signal.XValues;
            double[,] data = (double[,])signal.YValues; // Units: m/s^2 or (m/s^2)/N 
            if (keys.Length <= 1)
                return null;

            // Normalizing signals
            int length = data.GetLength(0);
            double[] realValues = new double[length];
            double[] imagValues = new double[length];
            for (int k = 0; k != length; ++k)
            {
                realValues[k] = data[k, 0];
                imagValues[k] = data[k, 1];
            }

            // Retrieve the info
            int channel = props["Channel id"];
            string originalRun = props["Original run"].AttributeMap["Contents"];
            string node = props["Point id node"];
            string component = props["Point id component"];
            string direction = props["Point direction absolute"];
            string dimension = props["Transducer id"];
            int numAverages = props["Number of averages"];
            int sign = 1;
            if (props["Point direction sign"] == "-")
                sign = -1;

            // Set the response
            Response response = new Response(type);
            // Data
            response.Keys = keys;
            response.RealValues = realValues;
            response.ImagValues = imagValues;
            // Props
            response.Path = path;
            response.OriginalRun = originalRun;
            response.Name = signal.Label;
            response.Node = node;
            response.Component = component;
            response.Direction = direction;
            response.Dimension = dimension;
            response.Channel = channel;
            response.NumAverages = numAverages;
            response.Sign = sign;

            return response;
        }

        // Create IBlock2 data
        private IBlock2 createSpectrumBlock(in Response response)
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

            // Set the attributes
            string sign = "+";
            if (response.Sign < 0)
                sign = "-";
            AttributeMap attributes = block.UserAttributes;
            attributes.Add("Channel id", response.Channel);
            attributes.Add("Channel name", "C" + response.Channel);
            attributes.Add("Channelgroup", "Measure");
            attributes.Add("DOF id", $"{response.Component}:{response.Node}:{sign}{response.Direction}");
            attributes.Add("Function class", "Spectrum");
            attributes.Add("Number of averages", response.NumAverages);
            attributes.Add("Point direction", sign + response.Direction);
            attributes.Add("Point direction absolute", response.Direction);
            attributes.Add("Point direction sign", sign);
            attributes.Add("Point id", $"{response.Component}:{response.Node}");
            attributes.Add("Point id component", response.Component);
            attributes.Add("Point id node", response.Node);
            block = block.ReplaceUserAttributes(attributes);

            return block;
        }



        private IApplication mApp;
        private IDatabase mDatabase;
        private IUnitSystem mUnitSystem;
        private string mLastPath;
    }
}
