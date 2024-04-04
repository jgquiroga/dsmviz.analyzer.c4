using Dsmviz.Analyzer.C4.Settings;
using Dsmviz.Datamodel.Dsi.Interface;
using Dsmviz.Util;
using System.Text.Json;

namespace Dsmviz.Analyzer.C4.Analysis
{
    public class Analyzer
    {
        private readonly IDsiModel _model;
        private readonly AnalyzerSettings _analyzerSettings;
        private readonly IProgress<ProgressInfo> _progress;
        private readonly Dictionary<string, C4Element> _elements = new Dictionary<string, C4Element>();
        private readonly List<C4Relationship> _relationships = new List<C4Relationship>();

        public Analyzer(IDsiModel model, AnalyzerSettings analyzerSettings, IProgress<ProgressInfo> progress)
        {
            _model = model;
            _analyzerSettings = analyzerSettings;
            _progress = progress;
        }

        private void FindRelationships(JsonElement parent)
        {
            if (!parent.TryGetProperty("relationships", out var relationships))
            {
                return;
            }

            foreach (var relationship in relationships.EnumerateArray())
            {
                var sourceId = relationship.GetProperty("sourceId").GetString();
                var destinationId = relationship.GetProperty("destinationId").GetString();
                var description = relationship.TryGetProperty("description", out var descriptionElement) ? descriptionElement.GetString() : null;

                if (description == null)
                {
                    description = "Uses";
                    Logger.LogUserMessage($"Relationship without description: {sourceId} -> {destinationId} ({description})");
                }
                else
                {
                    Logger.LogUserMessage($"Relationship: {sourceId} -> {destinationId} ({description})");
                }

                _relationships.Add(new C4Relationship { SourceId = sourceId, DestinationId = destinationId, Description = description });
            }
        }

        private void FindPeople(JsonElement parent, string parentName)
        {
            if (!parent.TryGetProperty("people", out var people))
            {
                return;
            }

            foreach (var person in people.EnumerateArray())
            {
                var id = person.GetProperty("id").GetString();
                var name = GetElementName(person);
                var type = person.TryGetProperty("type", out var typeElement) ? typeElement.GetString() : "Person";

                if (parentName != null)
                {
                    name = $"{parentName}.{name}";
                }

                Logger.LogUserMessage($"Person: {name}");

                _model.AddElement(name, type, null);
                _elements.Add(id, new C4Element { Id = id, Name = name });

                FindRelationships(person);
            }
        }


        private void FindSoftwareSystems(JsonElement parent, string parentName)
        {
            if (!parent.TryGetProperty("softwareSystems", out var softwareSystems))
            {
                return;
            }

            foreach (var softwareSystem in softwareSystems.EnumerateArray())
            {
                var id = softwareSystem.GetProperty("id").GetString();
                var name = GetElementName(softwareSystem);
                var type = softwareSystem.TryGetProperty("type", out var typeElement) ? typeElement.GetString() : "SoftwareSystem";
                var group = softwareSystem.TryGetProperty("group", out var groupElement) ? groupElement.GetString() : null;

                if (group != null)
                {
                    name = $"{group}.{name}";
                }

                if (parentName != null)
                {
                    name = $"{parentName}.{name}";
                }

                Logger.LogUserMessage($"Software system: {name}");

                _model.AddElement(name, type, null);
                _elements.Add(id, new C4Element { Id = id, Name = name });

                FindContainers(softwareSystem, name);

                FindRelationships(softwareSystem);
            }
        }

        private void FindContainers(JsonElement parent, string parentName)
        {
            if (!parent.TryGetProperty("containers", out var containers))
            {
                return;
            }

            foreach (var container in containers.EnumerateArray())
            {
                var id = container.GetProperty("id").GetString();
                var name = GetElementName(container);
                var type = container.TryGetProperty("type", out var typeElement) ? typeElement.GetString() : "Container";

                // Groups can not be added as a relation in the model
                // They only act as a partitioning mechanism
                var group = container.TryGetProperty("group", out var groupElement) ? groupElement.GetString() : null;

                if (group != null)
                {
                    name = $"{group}.{name}";
                }

                if (parentName != null)
                {
                    name = $"{parentName}.{name}";
                }

                Logger.LogUserMessage($"Container: {name}");

                _model.AddElement(name, type, null);
                _elements.Add(id, new C4Element { Id = id, Name = name });

                FindComponents(container, name);

                FindRelationships(container);
            }
        }

        private void FindComponents(JsonElement parent, string parentName)
        {
            if (!parent.TryGetProperty("components", out var components))
            {
                return;
            }

            foreach (var component in components.EnumerateArray())
            {
                var id = component.GetProperty("id").GetString();
                var name = GetElementName(component);
                var type = component.TryGetProperty("type", out var typeElement) ? typeElement.GetString() : "Component";

                if (parentName != null)
                {
                    name = $"{parentName}.{name}";
                }

                Logger.LogUserMessage($"Component: {name}");

                _model.AddElement(name, type, null);
                _elements.Add(id, new C4Element { Id = id, Name = name });

                FindCodeElements(component, name);

                FindRelationships(component);
            }
        }

        private void FindCodeElements(JsonElement parent, string parentName)
        {
            if (!parent.TryGetProperty("codeElements", out var codeElements))
            {
                return;
            }

            foreach (var codeElement in codeElements.EnumerateArray())
            {
                var id = codeElement.GetProperty("id").GetString();
                var name = GetElementName(codeElement);
                var type = codeElement.TryGetProperty("type", out var typeElement) ? typeElement.GetString() : "CodeElement";

                if (parentName != null)
                {
                    name = $"{parentName}.{name}";
                }

                Logger.LogUserMessage($"Code element: {name}");

                _model.AddElement(name, type, null);
                _elements.Add(id, new C4Element { Id = id, Name = name });

                FindRelationships(codeElement);
            }
        }

        private void FindChildElements(JsonElement parent, string parentName)
        {
            if (!parent.TryGetProperty("children", out var children))
            {
                return;
            }

            foreach (var child in children.EnumerateArray())
            {
                var id = child.GetProperty("id").GetString();
                var name = GetElementName(child);
                var type = child.TryGetProperty("type", out var typeElement) ? typeElement.GetString() : "Child";

                if (parentName != null)
                {
                    name = $"{parentName}.{name}";
                }

                Logger.LogUserMessage($"Child: {name}");

                _model.AddElement(name, type, null);
                _elements.Add(id, new C4Element { Id = id, Name = name });

                // Recursively add children
                FindChildElements(child, name);

                FindContainerInstances(child, name);

                FindInfrastructureNodes(child, name);

                FindRelationships(child);
            }
        }

        private void FindInfrastructureNodes(JsonElement parent, string parentName)
        {
            if (!parent.TryGetProperty("infrastructureNodes", out var infrastructureNodes))
            {
                return;
            }

            foreach (var infrastructureNode in infrastructureNodes.EnumerateArray())
            {
                var id = infrastructureNode.GetProperty("id").GetString();
                var name = GetElementName(infrastructureNode);
                var type = infrastructureNode.TryGetProperty("type", out var typeElement) ? typeElement.GetString() : "InfrastructureNode";

                if (parentName != null)
                {
                    name = $"{parentName}.{name}";
                }

                Logger.LogUserMessage($"Infrastructure node: {name}");

                _model.AddElement(name, type, null);
                _elements.Add(id, new C4Element { Id = id, Name = name });

                FindChildElements(infrastructureNode, name);

                FindContainerInstances(infrastructureNode, name);

                FindRelationships(infrastructureNode);
            }
        }

        private void FindDeploymentNodes(JsonElement parent, string parentName)
        {
            if (!parent.TryGetProperty("deploymentNodes", out var deploymentNodes))
            {
                return;
            }

            foreach (var deploymentNode in deploymentNodes.EnumerateArray())
            {
                var id = deploymentNode.GetProperty("id").GetString();
                var name = GetElementName(deploymentNode);
                var type = deploymentNode.TryGetProperty("type", out var typeElement) ? typeElement.GetString() : "DeploymentNode";
                var environment = deploymentNode.TryGetProperty("environment", out var environmentElement) ? environmentElement.GetString() : "EnvironmentDefault";

                name = $"Deployments.{environment}.{name}";

                if (parentName != null)
                {
                    name = $"{parentName}.{name}";
                }

                Logger.LogUserMessage($"Deployment node: {name}");

                _model.AddElement(name, type, null);
                _elements.Add(id, new C4Element { Id = id, Name = name });

                FindChildElements(deploymentNode, name);

                FindContainerInstances(deploymentNode, name);

                FindInfrastructureNodes(deploymentNode, name);

                FindRelationships(deploymentNode);
            }
        }

        private void FindContainerInstances(JsonElement parent, string parentName)
        {
            if (!parent.TryGetProperty("containerInstances", out var containerInstances))
            {
                return;
            }

            foreach (var containerInstance in containerInstances.EnumerateArray())
            {
                var id = containerInstance.GetProperty("id").GetString();
                var name = GetElementName(containerInstance);
                var type = containerInstance.TryGetProperty("type", out var typeElement) ? typeElement.GetString() : "ContainerInstance";
                var containerId = containerInstance.TryGetProperty("containerId", out var containerIdElement) ? containerIdElement.GetString() : null;

                if (parentName != null)
                {
                    name = $"{parentName}.{name}";
                }

                Logger.LogUserMessage($"Container instance: {name}");

                _model.AddElement(name, type, null);
                _elements.Add(id, new C4Element { Id = id, Name = name });

                if (containerId != null)
                {
                    _relationships.Add(new C4Relationship
                    {
                        SourceId = containerId,
                        DestinationId = id,
                        Description = "InstanceOf"
                    });
                }

                FindRelationships(containerInstance);
            }
        }

        private void RegisterRelationships()
        {
            foreach (var relationship in _relationships)
            {
                if (_elements.TryGetValue(relationship.SourceId, out var sourceElement) && _elements.TryGetValue(relationship.DestinationId, out var destinationElement))
                {
                    _model.AddRelation(sourceElement.Name, destinationElement.Name, relationship.Description, 1, null);
                }
            }
        }

        private string GetElementName(JsonElement element)
        {
            var id = element.GetProperty("id").GetString();
            var name = element.TryGetProperty("name", out var nameElement) ? nameElement.GetString() : null;

            if (!string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            if (element.TryGetProperty("properties", out var propertiesElement))
            {
                var identifier = propertiesElement.TryGetProperty("structurizr.dsl.identifier", out var identifierElement) ? identifierElement.GetString() : null;
                if (identifier != null)
                {
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        name = identifier;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                name = $"$Unnamed{id}";
            }

            return name;
        }

        public void Analyze()
        {
            var workspace = _analyzerSettings.Input.Workspace;

            FileInfo fileInfo = new FileInfo(workspace);
            if (!fileInfo.Exists)
            {
                Logger.LogError($"Workspace file '{workspace}' does not exist.");
            }

            using (FileStream stream = fileInfo.Open(FileMode.Open))
            {
                // Read a C4 json model from the workspace file
                JsonElement workspaceFile = JsonDocument.Parse(stream).RootElement;
                var model = workspaceFile.GetProperty("model");

                FindPeople(model, null);

                FindSoftwareSystems(model, null);

                FindDeploymentNodes(model, null);

                RegisterRelationships();
            }
        }
    }

    public class C4Element
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class C4Relationship
    {
        public string SourceId { get; set; }

        public string DestinationId { get; set; }

        public string Description { get; set; }
    }
}
