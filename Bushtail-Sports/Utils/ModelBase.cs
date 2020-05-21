

namespace Bushtail_Sports.Utils
{
    public class ModelBase
    {
        public event DataChangedHandler DataChanged;
        public delegate void DataChangedHandler();

        protected bool SetFieldData<T>(ref T storage, T value)
        {
            if (Equals(storage, value)) return false;

            storage = value;
            DataChanged();
            return true;
        }
    }
}
