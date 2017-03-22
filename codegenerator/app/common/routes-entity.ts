/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";

    angular.module('entityRoutes', []).config(entityRoutes);

    entityRoutes.$inject = ["$stateProvider"];
    function entityRoutes($stateProvider) {

        $stateProvider
            .state("app.codeReplacement", {
                url: "/projects/:projectId/entities/:entityId/codereplacements/:codeReplacementId",
                templateUrl: "/app/codereplacements/codereplacement.html",
                controller: "codeReplacement",
                controllerAs: "vm",
                ncyBreadcrumb: {
                    parent: "app.entity",
                    label: "{{vm.codeReplacement.purpose}}"
                }
            }).state("app.codeReplacements", {
                url: "/codereplacements",
                templateUrl: "/app/codereplacements/codereplacements.html",
                controller: "codeReplacements",
                controllerAs: "vm",
                ncyBreadcrumb: {
                    label: "Code Replacements"
                }
            }).state("app.entity", {
                url: "/projects/:projectId/entities/:entityId",
                templateUrl: "/app/entities/entity.html",
                controller: "entity",
                controllerAs: "vm",
                ncyBreadcrumb: {
                    parent: "app.project",
                    label: "{{vm.entity.name}}"
                }
            }).state("app.entities", {
                url: "/entities",
                templateUrl: "/app/entities/entities.html",
                controller: "entities",
                controllerAs: "vm",
                ncyBreadcrumb: {
                    label: "Entities"
                }
            }).state("app.field", {
                url: "/projects/:projectId/entities/:entityId/fields/:fieldId",
                templateUrl: "/app/fields/field.html",
                controller: "field",
                controllerAs: "vm",
                ncyBreadcrumb: {
                    parent: "app.entity",
                    label: "{{vm.field.name}}"
                }
            }).state("app.fields", {
                url: "/fields",
                templateUrl: "/app/fields/fields.html",
                controller: "fields",
                controllerAs: "vm",
                ncyBreadcrumb: {
                    label: "Fields"
                }
            }).state("app.lookup", {
                url: "/projects/:projectId/lookups/:lookupId",
                templateUrl: "/app/lookups/lookup.html",
                controller: "lookup",
                controllerAs: "vm",
                ncyBreadcrumb: {
                    parent: "app.project",
                    label: "{{vm.lookup.name}}"
                }
            }).state("app.lookups", {
                url: "/lookups",
                templateUrl: "/app/lookups/lookups.html",
                controller: "lookups",
                controllerAs: "vm",
                ncyBreadcrumb: {
                    label: "Lookups"
                }
            }).state("app.lookupOption", {
                url: "/projects/:projectId/lookups/:lookupId/lookupoptions/:lookupOptionId",
                templateUrl: "/app/lookupoptions/lookupoption.html",
                controller: "lookupOption",
                controllerAs: "vm",
                ncyBreadcrumb: {
                    parent: "app.lookup",
                    label: "{{vm.lookupOption.name}}"
                }
            }).state("app.lookupOptions", {
                url: "/lookupoptions",
                templateUrl: "/app/lookupoptions/lookupoptions.html",
                controller: "lookupOptions",
                controllerAs: "vm",
                ncyBreadcrumb: {
                    label: "Lookup Options"
                }
            }).state("app.project", {
                url: "/projects/:projectId",
                templateUrl: "/app/projects/project.html",
                controller: "project",
                controllerAs: "vm",
                ncyBreadcrumb: {
                    parent: "app.projects",
                    label: "{{vm.project.name}}"
                }
            }).state("app.projects", {
                url: "/",
                templateUrl: "/app/projects/projects.html",
                controller: "projects",
                controllerAs: "vm",
                ncyBreadcrumb: {
                    label: "Projects"
                }
            }).state("app.relationship", {
                url: "/projects/:projectId/entities/:entityId/relationships/:relationshipId",
                templateUrl: "/app/relationships/relationship.html",
                controller: "relationship",
                controllerAs: "vm",
                ncyBreadcrumb: {
                    parent: "app.entity",
                    label: "{{vm.relationship.collectionName}}"
                }
            }).state("app.relationships", {
                url: "/relationships",
                templateUrl: "/app/relationships/relationships.html",
                controller: "relationships",
                controllerAs: "vm",
                ncyBreadcrumb: {
                    label: "Relationships"
                }
            }).state("app.relationshipField", {
                url: "/projects/:projectId/entities/:entityId/relationships/:relationshipId/relationshipfields/:relationshipFieldId",
                templateUrl: "/app/relationshipfields/relationshipfield.html",
                controller: "relationshipField",
                controllerAs: "vm",
                ncyBreadcrumb: {
                    parent: "app.relationship",
                    label: "{{'Relationship Field'}}"
                }
            }).state("app.relationshipFields", {
                url: "/relationshipfields",
                templateUrl: "/app/relationshipfields/relationshipfields.html",
                controller: "relationshipFields",
                controllerAs: "vm",
                ncyBreadcrumb: {
                    label: "Relationship Fields"
                }
            });
    }

})();
