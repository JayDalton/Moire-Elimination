#pragma once

#include "pch.h"

namespace ChartWRCLibrary {
	namespace Commands {

		using namespace Platform;
		using namespace Windows::Foundation;
		using namespace Windows::Foundation::Metadata;
		using namespace Windows::UI::Xaml;
		using namespace Windows::UI::Xaml::Data;
		using namespace Windows::UI::Xaml::Input;
		using namespace Windows::UI::Xaml::Interop;

		public delegate void ExecuteDelegate(Object^ parameter);
		public delegate bool CanExecuteDelegate(Object^ parameter);

		[WebHostHidden]
		public ref class DelegateCommand sealed : public ICommand
		{
		private:
			ExecuteDelegate^ executeDelegate;
			CanExecuteDelegate^ canExecuteDelegate;
			bool lastCanExecute;

		public:
			DelegateCommand(ExecuteDelegate^ execute, CanExecuteDelegate^ canExecute);

			virtual event EventHandler<Object^>^ CanExecuteChanged;
			virtual void Execute(Object^ parameter);
			virtual bool CanExecute(Object^ parameter);
		};


		DelegateCommand::DelegateCommand(ExecuteDelegate^ execute, CanExecuteDelegate^ canExecute)
			: executeDelegate(execute), canExecuteDelegate(canExecute)
		{
		}

		void DelegateCommand::Execute(Object^ parameter)
		{
			if (executeDelegate != nullptr)
			{
				executeDelegate(parameter);
			}
		}

		bool DelegateCommand::CanExecute(Object^ parameter)
		{
			if (canExecuteDelegate == nullptr)
			{
				return true;
			}

			bool canExecute = canExecuteDelegate(parameter);

			if (lastCanExecute != canExecute)
			{
				lastCanExecute = canExecute;
				CanExecuteChanged(this, nullptr);
			}

			return lastCanExecute;
		}
	}
}

