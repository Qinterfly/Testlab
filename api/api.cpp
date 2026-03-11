#include "api.h"
#include "project.h"

namespace Testlab
{
	IProject* createProject()
	{
		IProject* pProject = new Project();
		return pProject;
	}

	IProject* openProject(std::filesystem::path const& pathFile)
	{
		IProject* pProject = new Project(pathFile);
		return pProject;
	}
}