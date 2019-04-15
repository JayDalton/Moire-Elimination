#pragma once

#include "pch.h"

namespace ChartWRCLibrary {
	namespace Converter {

		using namespace Platform;
		using namespace Windows::UI::Xaml::Data;
		using namespace Windows::UI::Xaml::Interop;

		public ref class RatingConverter sealed : IValueConverter
		{
		public:

			virtual Object^ Convert(Object^ value, TypeName targetType, Object^ parameter, String^ language)
			{
				auto boxedInt = dynamic_cast<Box<int>^>(value);
				auto intValue = boxedInt != nullptr ? boxedInt->Value : 1;

				return "Rating : " + ref new String(std::wstring(intValue, '*').c_str());
			}

			virtual Object^ ConvertBack(Object^ value, TypeName targetType, Object^ parameter, String^ language)
			{
				return value;
			}
		};
	}
}