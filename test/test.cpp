#include "pch.h"
#include "project.h"

using namespace Testlab;

TEST(ProjectCase, Open)
{
	Project* project = new Project("../examples/beam.lms");
	auto responses = project->getSelectedResponses();
	EXPECT_TRUE(project->isValid());
}