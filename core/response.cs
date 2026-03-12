namespace Core
{
    public enum ResponseType
    {
        kUnknown,
        kDisp,
        kVeloc,
        kAccel,
        kForce
    }

    public class Response
    {
        public Response(ResponseType type)
        {
            Type = type;
        }

        public bool equals(Response another)
        {
            return Path == another.Path;
        }

        public bool isComplex()
        {
            return Keys.Length > 0 && RealValues.Length == ImagValues.Length;
        }

        public int Length { get { return RealValues.Length; } }

        // Data
        public double[] Keys;
        public double[] RealValues;
        public double[] ImagValues;
        // Info
        public readonly ResponseType Type;
        public string Path;
        public string OriginalRun;
        public string Name;
        public string Node;
        public string Component;
        public string Direction;
        public string Dimension;
        public int Channel;
        public int NumAverages;
        public int Sign;
        public string Transducer;
    }
}
