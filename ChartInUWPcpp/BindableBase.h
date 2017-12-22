#pragma once

using namespace Platform;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Metadata;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Data;
using namespace Windows::UI::Xaml::Input;
using namespace Windows::UI::Xaml::Interop;

namespace ChartInUWPcpp 
{


	//public delegate void PropertyChangedEventHandler();

	//[Windows::UI::Xaml::Data::BindableAttribute]
	//public ref class BindableBase : INotifyPropertyChanged
	//{
	//public:
	//	//BindableBase();
	//	//~BindableBase();
	//	virtual event PropertyChangedEventHandler^ PropertyChanged;
	//	//void RaisePropertyChanged(String property);
	//	property Platform::String^ Foo
	//	{
	//		Platform::String^ get();
	//		void set(Platform::String^ value);
	//	}

	//	property Platform::String^ Bar
	//	{
	//		Platform::String^ get();
	//		void set(Platform::String^ value);
	//	}
	//private:
	//	Platform::String^ m_Foo;
	//	Platform::String^ m_Bar;
	//protected:
	//	virtual void OnPropertyChanged(Platform::String^ propertyName);
	//	//{
	//	//	PropertyChanged(this, ref new PropertyChangedEventArgs(propertyName));
	//	//}
	//};


	//[Bindable]
	//public ref class MainViewModel sealed : BindableBase 
	//{
	//	property String^ Title
	//	{
	//		String^ get()
	//		{
	//			return "MVVM Hello World with Visual C++";
	//		}
	//	}

	//};


	//public event PropertyChangedEventHandler PropertyChanged;

	//// SetField (Name, value); // where there is a data member
	//protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] String property = null)
	//{
	//	if (EqualityComparer<T>.Default.Equals(field, value)) return false;
	//	field = value;
	//	RaisePropertyChanged(property);
	//	return true;
	//}

	//// SetField(()=> somewhere.Name = value; somewhere.Name, value) 
	//// Advanced case where you rely on another property
	//protected bool SetProperty<T>(T currentValue, T newValue, Action DoSet, [CallerMemberName] String property = null)
	//{
	//	if (EqualityComparer<T>.Default.Equals(currentValue, newValue)) return false;
	//	DoSet.Invoke();
	//	RaisePropertyChanged(property);
	//	return true;
	//}

	//protected void RaisePropertyChanged(string property)
	//{
	//	if (PropertyChanged != null)
	//	{
	//		PropertyChanged(this, new PropertyChangedEventArgs(property));
	//	}
	//}

}


