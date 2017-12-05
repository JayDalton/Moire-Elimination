#pragma once

#include <ppltasks.h>
#include <concurrent_vector.h>
#include <collection.h>
#include <ppl.h>
#include <amp.h>
#include <amp_math.h>

#include "Source\FftRealPair.hpp"

using namespace concurrency;
using namespace Platform;
using namespace Platform::Collections;
using namespace Windows::Foundation::Collections;
using namespace Windows::Foundation;
using namespace Windows::UI::Core;

namespace ChartWRCLibrary
{
	public delegate void FourierCalcHandler(int result);

	public ref class FftHelper sealed 
	{
	private:
		FftRealPair* m_fourier;
	public:
		FftHelper();
		event FourierCalcHandler^ fourierEvent;
		IVector<double> ^SetContent(double rows, double cols, IVector<double>^ data);
		IVector<double> ^SetContent(uint16 rows, uint16 cols, IVector<unsigned short>^ data);
	};

	public value struct ImageMatrix {
		int Cols;
		int Rows;
		//std::vector<int> Data;
	};

	public value struct MyStruct
	{
		Platform::String^ Name;
		int Number;
		double Avarage;
	};

	public delegate void PrimeFoundHandler(int result);

	public ref class Class1 sealed
    {
	
	private:
		MyStruct m_struct;
		bool is_prime(int n);
		Windows::UI::Core::CoreDispatcher^ m_dispatcher;
	
	public:
		Class1();

		// Synchronous method.
		IVector<double>^  ComputeResult(double input);

		// Asynchronous methods
		IAsyncOperationWithProgress<IVector<int>^, double>^ GetPrimesOrdered(int first, int last);
		IAsyncActionWithProgress<double>^ GetPrimesUnordered(int first, int last);

		// Event whose type is a delegate "class"
		event PrimeFoundHandler^ primeFoundEvent;		
		
		// samples
		double LogCalc(double input) {
			return std::log(input);
		}

		property MyStruct Structur {
			MyStruct get() { return m_struct; }
			void set(MyStruct _struct) { m_struct = _struct; }
		}

		IMap<int, Platform::String^> ^GetMap(void) 
		{
			IMap<int, Platform::String^> ^result = ref new Map<int, Platform::String^>;
			result->Insert(1, "Hello");
			result->Insert(2, " ");
			result->Insert(3, "Wolrd");
			result->Insert(4, "!");

			return result;
		}

		IVector<int> ^SortVector(IVector<int> ^vec) 
		{
			std::sort(begin(vec), end(vec));
			return vec;
		}
    };

}
