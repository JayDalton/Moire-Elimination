#pragma once

#include "pch.h"

namespace ChartWRCLibrary {
	namespace ViewModels {

		using namespace Platform;
		using namespace Windows::Foundation;
		using namespace Windows::Foundation::Metadata;
		using namespace Windows::UI::Xaml;
		using namespace Windows::UI::Xaml::Data;
		using namespace Windows::UI::Xaml::Input;
		using namespace Windows::UI::Xaml::Interop;

		[Windows::UI::Xaml::Data::Bindable]
		public ref class BindableBase : DependencyObject, INotifyPropertyChanged
		{
		public:
			virtual event PropertyChangedEventHandler^ PropertyChanged;

		protected:
			virtual void OnPropertyChanged(String^ propertyName);
		};

		void BindableBase::OnPropertyChanged(String^ propertyName)
		{
			PropertyChanged(this, ref new PropertyChangedEventArgs(propertyName));
		}
	}
}