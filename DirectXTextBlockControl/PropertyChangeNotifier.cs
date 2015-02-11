using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace DirectXTextBlockControl
{
    public sealed class PropertyChangeNotifier :
        DependencyObject,
        IDisposable
    {
        #region Member Variables

        //private readonly WeakReference _propertySource;
        private readonly DependencyObject _propertySource;

        #endregion // Member Variables

        #region Constructor

        public PropertyChangeNotifier(DependencyObject propertySource, string path)
            : this(propertySource, new PropertyPath(path))
        {
        }

        public PropertyChangeNotifier(DependencyObject propertySource, PropertyPath property)
        {
            if (null == propertySource)

                throw new ArgumentNullException("propertySource")
            ;

            if (null == property)

                throw new ArgumentNullException("property")
            ;

            //_propertySource = new WeakReference(propertySource);
            _propertySource = propertySource;

            var binding = new Binding();
            binding.Path = property;
            binding.Mode = BindingMode.OneWay;
            binding.Source = propertySource;

            BindingOperations.SetBinding(this, ValueProperty, binding);
        }

        #endregion // Constructor

        #region PropertySource

        public DependencyObject PropertySource
        {
            get
            {
                try
                {
                    // note, it is possible that accessing the target property

                    // will result in an exception so i’ve wrapped this check

                    // in a try catch

                    //return _propertySource.IsAlive
                    //    ? _propertySource.Target as DependencyObject
                    //    : null;

                    return _propertySource;
                }

                catch
                {
                    return null;
                }
            }
        }

        #endregion // PropertySource

        #region Value

        /// <summary>
        ///     Identifies the <see cref="Value" /> dependency property
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value",
            typeof(object), typeof(PropertyChangeNotifier),
            new PropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));


        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var notifier = (PropertyChangeNotifier)d;

            try
            {
                if (null != notifier.ValueChanged)

                    notifier.ValueChanged(notifier, EventArgs.Empty);

            }
            catch (Exception)
            {
            }
        }


        /// <summary>
        ///     Returns/sets the value of the property
        /// </summary>
        /// <seealso cref="ValueProperty"/>
        public object Value
        {
            get { return GetValue(ValueProperty); }

            set { SetValue(ValueProperty, value); }
        }

        #endregion //Value

        #region Events

        public event EventHandler ValueChanged;

        #endregion // Events

        #region IDisposable Members

        public void Dispose()
        {
            // Not available so ...  BindingOperations.ClearBinding(this, ValueProperty);

            this.ClearValue(ValueProperty);
        }

        #endregion
    }
}
