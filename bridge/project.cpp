
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

Core::Response^ convert(Response const& response);
Core::ResponsePoint^ convert(ResponsePoint const& point);
Core::ResponseUnit^ convert(ResponseUnit const& unit);

List<Core::Response^>^ convert(std::vector<Response> const& responses);

// From managed
std::wstring convert(String^ string);
std::vector<int> convert(array<int>^ array);
std::vector<double> convert(array<double>^ array);
std::vector<std::vector<int>> convert(array<int, 2>^ array);

Response convert(Core::Response^ response);
ResponsePoint convert(Core::ResponsePoint^ point);
ResponseUnit convert(Core::ResponseUnit^ unit);
Geometry convert(Core::Geometry^ geometry);
Component convert(Core::Component^ component);
Node convert(Core::Node^ component);

std::vector<Response> convert(List<Core::Response^>^ responses);
std::vector<Component> convert(List<Core::Component^>^ components);
std::vector<Node> convert(List<Core::Node^>^ nodes);

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

std::vector<Response> Project::getResponses(std::vector<std::wstring> const& paths)
{
	List<String^>^ cPaths = convert(paths);
	List<Core::Response^>^ responses = mpImpl->manager->getResponses(cPaths);
	std::vector<Response> result = convert(responses);
	return result;

}

std::vector<Response> Project::getSelectedResponses()
{
	List<Core::Response^>^ responses = mpImpl->manager->getSelectedResponses();
	std::vector<Response> result = convert(responses);
	return result;
}

bool Project::addResponses(std::vector<Response> const& responses, std::wstring const& path)
{
	List<Core::Response^>^ cResponses = convert(responses);
	String^ cPath = convert(path);
	return mpImpl->manager->addResponses(cResponses, cPath);
}

Geometry Project::getGeometry()
{
	Core::Geometry^ geometry = mpImpl->manager->getGeometry();
	Geometry result = convert(geometry);
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

Core::Response^ convert(Response const& response)
{
	Core::Response^ result = gcnew Core::Response();

	// Data
	result->Keys = convert(response.keys);
	result->RealValues = convert(response.realValues);
	result->ImagValues = convert(response.imagValues);

	// Header
	Core::ResponseHeader^ cHeader = result->Header;
	ResponseHeader const& rHeader = response.header;
	cHeader->Type = (Core::ResponseType)rHeader.type;
	cHeader->Path = convert(rHeader.path);
	cHeader->OriginalRun = convert(rHeader.originalRun);
	cHeader->Name = convert(rHeader.name);
	cHeader->Point = convert(rHeader.point);
	cHeader->RefPoint = convert(rHeader.refPoint);
	cHeader->Unit = convert(rHeader.unit);
	cHeader->Channel = rHeader.channel;
	cHeader->NumAverages = rHeader.numAverages;
	cHeader->Dimension = convert(rHeader.dimension);
	cHeader->Transducer = convert(rHeader.transducer);
	cHeader->Comment = convert(rHeader.comment);

	return result;
}

Core::ResponsePoint^ convert(ResponsePoint const& point)
{
	Core::ResponsePoint^ result = gcnew Core::ResponsePoint();
	result->Name = convert(point.name);
	result->Node = convert(point.node);
	result->Component = convert(point.component);
	result->Direction = (Core::Direction)point.direction;
	result->Sign = point.sign;
	return result;
}

Core::ResponseUnit^ convert(ResponseUnit const& unit)
{
	Core::ResponseUnit^ result = gcnew Core::ResponseUnit();
	result->Length = unit.length;
	result->Mass = unit.mass;
	result->Time = unit.time;
	result->Scale = unit.scale;
	result->Name = convert(unit.name);
	return  result;
}

List<Core::Response^>^ convert(std::vector<Response> const& responses)
{
	List<Core::Response^>^ result = gcnew List<Core::Response^>(responses.size());
	for (const auto& response : responses)
		result->Add(convert(response));
	return result;
}

std::wstring convert(String^ string)
{
	if (string)
		return marshal_as<std::wstring>(string);
	return std::wstring();
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

Response convert(Core::Response^ response)
{
	Response result;

	// Data
	result.keys = convert(response->Keys);
	result.realValues = convert(response->RealValues);
	result.imagValues = convert(response->ImagValues);

	// Header
	Core::ResponseHeader^ cHeader = response->Header;
	ResponseHeader& rHeader = result.header;
	rHeader.type = (ResponseType)cHeader->Type;
	rHeader.path = convert(cHeader->Path);
	rHeader.originalRun = convert(cHeader->OriginalRun);
	rHeader.name = convert(cHeader->Name);
	rHeader.point = convert(cHeader->Point);
	rHeader.refPoint = convert(cHeader->RefPoint);
	rHeader.unit = convert(cHeader->Unit);
	rHeader.channel = cHeader->Channel;
	rHeader.numAverages = cHeader->NumAverages;
	rHeader.dimension = convert(cHeader->Dimension);
	rHeader.transducer = convert(cHeader->Transducer);
	rHeader.comment = convert(cHeader->Comment);

	return result;
}

ResponsePoint convert(Core::ResponsePoint^ point)
{
	ResponsePoint result;
	result.name = convert(point->Name);
	result.node = convert(point->Node);
	result.component = convert(point->Component);
	result.direction = (Direction)point->Direction;
	result.sign = point->Sign;
	return result;
}

ResponseUnit convert(Core::ResponseUnit^ unit)
{
	ResponseUnit result;
	result.length = unit->Length;
	result.mass = unit->Mass;
	result.time = unit->Time;
	result.scale = unit->Scale;
	result.name = convert(unit->Name);
	return result;
}

Geometry convert(Core::Geometry^ geometry)
{
	Geometry result;
	result.components = convert(geometry->Components);
	return result;
}

Component convert(Core::Component^ component)
{
	Component result;
	result.name = convert(component->Name);
	result.coordinates = convert(component->Coordinates);
	result.angles = convert(component->Angles);
	result.nodes = convert(component->Nodes);
	result.lines = convert(component->Lines);
	result.trias = convert(component->Trias);
	result.quads = convert(component->Quads);
	return result;
}

Node convert(Core::Node^ node)
{
	Node result;
	result.name = convert(node->Name);
	result.coordinates = convert(node->Coordinates);
	result.angles = convert(node->Angles);
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

std::vector<Component> convert(List<Core::Component^>^ components)
{
	int numComponents = components->Count;
	std::vector<Component> result(numComponents);
	for (int i = 0; i != numComponents; ++i)
	{
		Core::Component^ component = components->ToArray()[i];
		result[i] = convert(component);
	}
	return result;
}

std::vector<Node> convert(List<Core::Node^>^ nodes)
{
	int numNodes = nodes->Count;
	std::vector<Node> result(numNodes);
	for (int i = 0; i != numNodes; ++i)
	{
		Core::Node^ node = nodes->ToArray()[i];
		result[i] = convert(node);
	}
	return result;
}