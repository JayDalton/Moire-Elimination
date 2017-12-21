#include "pch.h"
#include "BindableBase.h"

using namespace ChartInUWPcpp;

//BindableBase::BindableBase()
//{
//}
//
//
//BindableBase::~BindableBase()
//{
//}

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

void BindableBase::OnPropertyChanged(String^ propertyName)
{
	PropertyChanged(this, ref new PropertyChangedEventArgs(propertyName));
}
