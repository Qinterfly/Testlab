
#include <msclr/marshal_cppstd.h>
#include <msclr/gcroot.h>

#include "project.h"

using namespace System;
using namespace System::IO;
using namespace System::Collections::Generic;
using namespace msclr;
using namespace msclr::interop;
using namespace Testlab;

// To managed
String^ convert(std::wstring const& string);

// From managed
std::wstring convert(String^ string);
std::vector<double> convert(array<double>^ array);
Response convert(Core::Response^ response);
std::vector<Response> convert(List<Core::Response^>^ responses);

class Project::Impl
{
public:
	Impl()
	{
		manager = gcnew Core::Manager();
	};
	gcroot<Core::Manager^> manager;
};

Project::Project()
{
	mpImpl = new Impl;
	mpImpl->manager->initialize();
}

Project::Project(fs::path const& pathFile)
{
	String^ cPathFile = convert(pathFile.wstring());
	cPathFile = Path::GetFullPath(cPathFile);
	mpImpl = new Impl;
	mpImpl->manager->openProject(cPathFile);
}

Project::~Project()
{
	delete mpImpl;
}

bool Project::isValid() const
{
	return mpImpl->manager->isValid();
}

void Project::createSection(std::wstring const& section, bool isSelect)
{
	String^ cSection = convert(section);
	mpImpl->manager->createSection(cSection, isSelect);
}

bool Project::isSectionExist(std::wstring const& section)
{
	String^ cSection = convert(section);
	return mpImpl->manager->isSectionExist(cSection);
}

void Project::createFolder(std::wstring const& section, std::wstring const& folder)
{
	String^ cSection = convert(section);
	String^ cFolder = convert(folder);
	mpImpl->manager->createFolder(cSection, cFolder);
}

bool Project::isFolderExist(std::wstring const& section, std::wstring const& folder)
{
	String^ cSection = convert(section);
	String^ cFolder = convert(folder);
	return mpImpl->manager->isFolderExist(cSection, cFolder);
}

std::vector<Response> Project::getSelectedResponses()
{
	List<Core::Response^>^ responses = mpImpl->manager->getSelectedResponses();
	std::vector<Response> result = convert(responses);
	return result;
}

String^ convert(std::wstring const& string)
{
	return marshal_as<String^>(string);
}

std::wstring convert(String^ string)
{
	return marshal_as<std::wstring>(string);
}

std::vector<double> convert(array<double>^ array)
{
	int numArray = array->Length;
	std::vector<double> result(numArray);
	for (int i = 0; i != numArray; ++i)
		result[i] = array[i];
	return result;
}

Response convert(Core::Response^ response)
{
	Response result;
	// Data
	result.keys = convert(response->Keys);
	result.realValues = convert(response->RealValues);
	result.imagValues = convert(response->ImagValues);
	// Info
	result.type = (ResponseType)response->Type;
	result.name = convert(response->Name);
	result.path = convert(response->Path);
	result.originalRun = convert(response->OriginalRun);
	result.node = convert(response->Node);
	result.direction = convert(response->Direction);
	return result;
}

std::vector<Response> convert(List<Core::Response^>^ responses)
{
	int numResponses = responses->Count;
	std::vector<Response> result(numResponses);
	for (int i = 0; i != numResponses; ++i)
	{
		Core::Response^ response = responses->ToArray()[i];
		result[i] = convert(response);
	}
	return result;
}
