#include "pch.h"
#include "api.h"

using namespace Testlab;

static IProject* spProject;
static std::vector<IResponse*> sResponses;

TEST(ProjectCase, Open)
{
	spProject = openProject("../examples/beam.lms");
	EXPECT_TRUE(spProject->isValid());
}

TEST(ProjectCase, GetResponses)
{
	std::wstring basePath = L"Section1/Record/ResponsesSpectra/";
	std::vector<std::wstring> paths = { L"Harmonic Spectrum Beam:1:+Z", L"Harmonic Spectrum Beam:2:+Z" };
	for (std::wstring& v : paths)
		v = basePath + v;
	sResponses = spProject->getResponses(paths);
	EXPECT_TRUE(sResponses.size() == 2);
}

TEST(ProjectCase, AddResponses)
{
	std::wstring section = L"Section1";
	std::wstring folder = L"Test";
	std::wstring path = section + L"/" + folder;
	spProject->createFolder(section, folder);
	spProject->addResponses(sResponses, path);
}