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

        public int Length { get { return RealValues.Length; } }

        // Data
        public double[] Keys;
        public double[] RealValues;
        public double[] ImagValues;
        // Info
        public readonly ResponseType Type;
        public string Path;
        public string Name;
        public string OriginalRun;
        public string Node;
        public string Component;
        public string Direction;
    }
}
