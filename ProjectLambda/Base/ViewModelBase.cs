using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLambda.Base
{
    public abstract class ViewModelBase
        : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<TProp>(MethodBase methodBase, ref TProp oldValue, TProp newValue)
        {
            if ((oldValue == null) && (newValue == null))
            {
                return false;
            }
            if ((oldValue == null) && (newValue != null) || !oldValue.Equals(newValue))
            {
                oldValue = newValue;
                string methodName = methodBase.Name;
                if (!methodName.StartsWith("set_"))
                {
                    throw new Exception(methodName + " is not a set-property.");
                }
                string propertyName = methodName.Substring(4);
                OnPropertyChanged(propertyName);
                return true;
            }

            return false;
        }

        protected void OnPropertyChanged(PropertyInfo propInfo)
        {
            OnPropertyChanged(propInfo.Name);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
