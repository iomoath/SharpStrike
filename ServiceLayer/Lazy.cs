using System;

namespace ServiceLayer
{
    internal class Lazy<T> where T : class
    {
        private readonly Func<T> _factoryMethod;
        private readonly object _syncLock = new object();
        private T _value;

        public T Value
        {
            get
            {
                if (_value == null)
                {
                    lock (_syncLock)
                    {
                        if (_value == null)
                        {
                            _value = _factoryMethod();
                        }
                    }
                }
                return _value;
            }
            set => _value = value;
        }

        public Lazy(Func<T> factoryMethod)
        {
            _factoryMethod = factoryMethod ?? throw new ArgumentNullException(nameof(factoryMethod));
        }
    }
}
