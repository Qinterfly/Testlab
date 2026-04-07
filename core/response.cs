namespace Core
{
    public enum Direction
    {
        kNone,
        kX,
        kY,
        kZ
    }

    public enum ResponseType
    {
        kNone,
        kDisp,
        kVeloc,
        kAccel,
        kForce
    }

    public class ResponsePoint
    {
        public string Name;
        public string Node;
        public string Component;
        public Direction Direction;
        public int Sign;
    }

    public class ResponseUnit
    {
        public ResponseUnit()
        {
            Length = 0;
            Mass = 0;
            Time = 0;
            Scale = 1.0;
        }
        public ResponseUnit(int L, int M, int T, double S, string N)
        {
            Length = L;
            Mass = M;
            Time = T;
            Scale = S;
            Name = N;
        }

        public int Length;
        public int Mass;
        public int Time;
        public double Scale;
        public string Name;
    }

    public class ResponseHeader
    {
        public ResponseHeader()
        {
            Point = new ResponsePoint();
            RefPoint = new ResponsePoint();
            Unit = new ResponseUnit();
        }

        public ResponseType Type;
        public string Path;
        public string OriginalRun;
        public string Name;
        public ResponsePoint Point;
        public ResponsePoint RefPoint;
        public ResponseUnit Unit;
        public int Channel;
        public int NumAverages;
        public string Dimension;
        public string Transducer;
        public string Comment;
    }

    public class Response
    {
        public Response()
        {
            Header = new ResponseHeader();
        }

        public bool equals(Response another)
        {
            return Header.Path == another.Header.Path;
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
        // Header
        public ResponseHeader Header;
    }
}
