using System.ComponentModel;
using System.Threading;

namespace ExpireAlert
{
    public class BaseVM : INotifyPropertyChanged
    {
        public BaseVM()
        {
            m_syncCtx = SynchronizationContext.Current;
        }

        // 触发属性变更事件
        protected void OnPropertyChanged(string strProperty)
        {
            if (PropertyChanged != null)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(strProperty);
                PropertyChanged(this, e);
            }
        }

        // 实现INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        // 同步环境
        protected readonly SynchronizationContext m_syncCtx;
    } 
}
