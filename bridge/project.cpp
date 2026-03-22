
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
List<String^>^ convert(std::vector<std::wstring> const& strings);
array<double>^ convert(std::vector<double> const& data);
List<Core::Response^>^ convert(std::vector<IResponse*> responses);

// From managed
std::wstring convert(String^ string);
std::vector<int> convert(array<int>^ array);
std::vector<double> convert(array<double>^ array);
std::vector<std::vector<int>> convert(array<int, 2>^ array);

IResponse* convert(Core::Response^ response);
IGeometry* convert(Core::Geometry^ geometry);
IComponent* convert(Core::Component^ component);
INode* convert(Core::Node^ component);

std::vector<IResponse*> convert(List<Core::Response^>^ responses);
std::vector<IComponent*> convert(List<Core::Component^>^ components);
std::vector<INode*> convert(List<Core::Node^>^ nodes);

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

std::wstring Project::getPath()
{
	std::wstring result = convert(mpImpl->manager->getPath());
	return result;
}

std::wstring Project::getActiveSection()
{
	String^ cSection = mpImpl->manager->getActiveSection();
	std::wstring result = convert(cSection);
	return result;
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

std::vector<IResponse*> Project::getResponses(std::vector<std::wstring> const& paths)
{
	List<String^>^ cPaths = convert(paths);
	List<Core::Response^>^ responses = mpImpl->manager->getResponses(cPaths);
	std::vector<IResponse*> result = convert(responses);
	return result;

}

std::vector<IResponse*> Project::getSelectedResponses()
{
	List<Core::Response^>^ responses = mpImpl->manager->getSelectedResponses();
	std::vector<IResponse*> result = convert(responses);
	return result;
}

bool Project::addResponses(std::vector<IResponse*> const& responses, std::wstring const& path)
{
	List<Core::Response^>^ cResponses = convert(responses);
	String^ cPath = convert(path);
	return mpImpl->manager->addResponses(cResponses, cPath);
}

IGeometry* Project::getGeometry()
{
	Core::Geometry^ geometry = mpImpl->manager->getGeometry();
	IGeometry* result = convert(geometry);
	return result;
}

String^ convert(std::wstring const& string)
{
	return marshal_as<String^>(string);
}

List<String^>^ convert(std::vector<std::wstring> const& strings)
{
	List<String^>^ result = gcnew List<String^>(strings.size());
	for (const auto& v : strings)
		result->Add(convert(v));
	return result;
}

array<double>^ convert(std::vector<double> const& data)
{
	int numData = data.size();
	array<double>^ result = gcnew array<double>(numData);
	for (int i = 0; i != numData; ++i)
		result[i] = data[i];
	return result;
}

List<Core::Response^>^ convert(std::vector<IResponse*> responses)
{
	List<Core::Response^>^ result = gcnew List<Core::Response^>(responses.size());
	for (const auto& response : responses)
	{
		Core::Response^ item = gcnew Core::Response((Core::ResponseType)response->type);

		// Data
		item->Keys = convert(response->keys);
		item->RealValues = convert(response->realValues);
		item->ImagValues = convert(response->imagValues);

		// Info
		item->Path = convert(response->path);
		item->OriginalRun = convert(response->originalRun);
		item->Name = convert(response->name);
		item->Node = convert(response->node);
		item->Component = convert(response->component);
		item->Direction = convert(response->direction);
		item->Dimension = convert(response->dimension);
		item->Channel = response->channel;
		item->NumAverages = response->numAverages;
		item->Sign = response->sign;
		item->Transducer = convert(response->transducer);
		item->Comment = convert(response->comment);

		result->Add(item);
	}
	return result;
}

std::wstring convert(String^ string)
{
	return marshal_as<std::wstring>(string);
}

std::vector<int> convert(array<int>^ array)
{
	int numArray = array->Length;
	std::vector<int> result(numArray);
	for (int i = 0; i != numArray; ++i)
		result[i] = array[i];
	return result;
}

std::vector<double> convert(array<double>^ array)
{
	int numArray = array->Length;
	std::vector<double> result(numArray);
	for (int i = 0; i != numArray; ++i)
		result[i] = array[i];
	return result;
}

std::vector<std::vector<int>> convert(array<int, 2>^ array)
{
	int numRows = array->GetLength(0);
	int numCols = array->GetLength(1);
	std::vector<std::vector<int>> result(numRows);
	std::vector<int> row(numCols);
	for (int i = 0; i != numRows; ++i)
	{
		for (int j = 0; j != numCols; ++j)
			row[j] = array[i, j];
		result[i] = row;
	}
	return result;
}

IResponse* convert(Core::Response^ response)
{
	IResponse* result = new IResponse;

	// Data
	result->keys = convert(response->Keys);
	result->realValues = convert(response->RealValues);
	result->imagValues = convert(response->ImagValues);

	// Info
	result->type = (ResponseType)response->Type;
	result->path = convert(response->Path);
	result->originalRun = convert(response->OriginalRun);
	result->name = convert(response->Name);
	result->node = convert(response->Node);
	result->component = convert(response->Component);
	result->direction = convert(response->Direction);
	result->dimension = convert(response->Dimension);
	result->channel = response->Channel;
	result->numAverages = response->NumAverages;
	result->sign = response->Sign;
	result->transducer = convert(response->Transducer);
	result->comment = convert(response->Comment);

	return result;
}

IGeometry* convert(Core::Geometry^ geometry)
{
	IGeometry* result = new IGeometry;
	result->components = convert(geometry->Components);
	return result;
}

IComponent* convert(Core::Component^ component)
{
	IComponent* result = new IComponent;
	result->name = convert(component->Name);
	result->coordinates = convert(component->Coordinates);
	result->angles = convert(component->Angles);
	result->nodes = convert(component->Nodes);
	result->lines = convert(component->Lines);
	result->trias = convert(component->Trias);
	result->quads = convert(component->Quads);
	return result;
}

INode* convert(Core::Node^ node)
{
	INode* result = new INode;
	result->name = convert(node->Name);
	result->coordinates = convert(node->Coordinates);
	result->angles = convert(node->Angles);
	return result;
}

std::vector<IResponse*> convert(List<Core::Response^>^ responses)
{
	int numResponses = responses->Count;
	std::vector<IResponse*> result(numResponses);
	for (int i = 0; i != numResponses; ++i)
	{
		Core::Response^ response = responses->ToArray()[i];
		result[i] = convert(response);
	}
	return result;
}

std::vector<IComponent*> convert(List<Core::Component^>^ components)
{
	int numComponents = components->Count;
	std::vector<IComponent*> result(numComponents);
	for (int i = 0; i != numComponents; ++i)
	{
		Core::Component^ component = components->ToArray()[i];
		result[i] = convert(component);
	}
	return result;
}

std::vector<INode*> convert(List<Core::Node^>^ nodes)
{
	int numNodes = nodes->Count;
	std::vector<INode*> result(numNodes);
	for (int i = 0; i != numNodes; ++i)
	{
		Core::Node^ node = nodes->ToArray()[i];
		result[i] = convert(node);
	}
	return result;
}