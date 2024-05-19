namespace GameStateAndData
{
    public interface ISaveDataVisitable
    {
        void Accept(ISaveDataVisitor visitor);
    }
}