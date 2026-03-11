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

        public void clear()
        {
            mApp = null;
            mDatabase = null;
            mUnitSystem = null;
        }

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
            }
            catch
            {
                clear();
                return false;
            }
            return true;
        }

        public bool isValid()
        {
            return mApp != null;
        }

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
                        Response response = getResponse(pathSignal, signal, signal.Properties);
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

        private Response getResponse(in string path, in IBlock2 signal, in AttributeMap props)
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

            // Retrieving additional info
            double sign = 1.0;
            if (props["Point direction sign"] == "-")
                sign = -1.0;

            // Normalizing signals
            int length = data.GetLength(0);
            double[] realValues = new double[length];
            double[] imagValues = new double[length];
            for (int k = 0; k != length; ++k)
            {
                realValues[k] = data[k, 0] * sign;
                imagValues[k] = data[k, 1] * sign;
            }

            // Retrieve the info
            string originalRun = props["Original run"].AttributeMap["Contents"];
            string node = props["Point id node"];
            string component = props["Point id component"];
            string direction = props["Point direction absolute"];

            // Set the response
            Response response = new Response(type);
            response.Path = path;
            response.Name = signal.Label;
            response.OriginalRun = originalRun;
            response.Node = node;
            response.Component = component;
            response.Direction = direction;
            response.Keys = keys;
            response.RealValues = realValues;
            response.ImagValues = imagValues;
            return response;
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


        private IApplication mApp;
        private IDatabase mDatabase;
        private IUnitSystem mUnitSystem;
    }
}
