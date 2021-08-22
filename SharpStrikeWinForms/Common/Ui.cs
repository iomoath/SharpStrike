using System;
using System.Windows.Forms;

namespace SharpStrike
{
    internal class Ui
    {
        private Control _internalControl;
        public void Initialize(Control control)
        {
            _internalControl = control;
        }

        public void Invoke(Action action)
        {
            if (_internalControl.InvokeRequired)
            {
                _internalControl.BeginInvoke(action);
            }
            else
            {
                action();
            }
        }
    }
}