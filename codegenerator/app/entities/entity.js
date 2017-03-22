/// <reference path="../../scripts/typings/highlightjs/highlightjs.d.ts" />
/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";
    angular
        .module("app")
        .controller("entity", entity);
    entity.$inject = ["$scope", "$state", "$stateParams", "entityResource", "notifications", "appSettings", "$q", "errorService", "projectResource", "relationshipResource", "fieldResource", "codeReplacementResource"];
    function entity($scope, $state, $stateParams, entityResource, notifications, appSettings, $q, errorService, projectResource, relationshipResource, fieldResource, codeReplacementResource) {
        var vm = this;
        vm.loading = true;
        vm.user = null;
        vm.save = save;
        vm.delete = del;
        vm.appSettings = appSettings;
        vm.isNew = $stateParams.entityId === vm.appSettings.newGuid;
        vm.goToField = function (projectId, entityId, fieldId) { return $state.go("app.field", { projectId: projectId, entityId: entityId, fieldId: fieldId }); };
        vm.goToRelationship = function (entityId, relationshipId) { return $state.go("app.relationship", { projectId: $stateParams.projectId, entityId: entityId, relationshipId: relationshipId }); };
        vm.goToCodeReplacement = function (projectId, entityId, codeReplacementId) { return $state.go("app.codeReplacement", { projectId: projectId, entityId: entityId, codeReplacementId: codeReplacementId }); };
        vm.fieldsSortOptions = { stop: sortFields, handle: "i.sortable-handle" };
        vm.relationshipsAsParentSortOptions = { stop: sortParentRelationships, handle: "i.sortable-handle" };
        vm.codeReplacementsSortOptions = { stop: sortCodeReplacements, handle: "i.sortable-handle", axis: "y" };
        vm.CodeType = function (type) {
            var types = [{ id: 0, name: "Model" }, { id: 1, name: "DTO" }, { id: 2, name: "DbContext" }, { id: 3, name: "Controller" }, { id: 4, name: "BundleConfig" }, { id: 5, name: "AppRouter" }, { id: 6, name: "ApiResource" }, { id: 7, name: "ListHtml" }, { id: 8, name: "ListTypeScript" }, { id: 9, name: "EditHtml" }, { id: 10, name: "EditTypeScript" }];
            for (var i = 0; i < types.length; i++) {
                if (types[i].id === type)
                    return types[i].name;
            }
        };
        initPage();
        function initPage() {
            var promises = [];
            $q.all(promises)
                .then(function () {
                if (vm.isNew) {
                    vm.entity = new entityResource();
                    vm.entity.entityId = appSettings.newGuid;
                    vm.entity.projectId = $stateParams.projectId;
                    vm.entity.authorizationType = 1;
                    vm.entity.entityType = 0;
                    promises = [];
                    promises.push(projectResource.get({
                        projectId: $stateParams.projectId
                    }, function (data) {
                        vm.project = data;
                    }, function (err) {
                        if (err.status === 404) {
                            notifications.error("The requested project does not exist.", "Error");
                        }
                        else {
                            notifications.error("Failed to load the project.", "Error", err);
                        }
                        $state.go("app.project", { projectId: $stateParams.projectId });
                    })
                        .$promise);
                    $q.all(promises).finally(function () { return vm.loading = false; });
                }
                else {
                    promises = [];
                    promises.push(entityResource.get({
                        entityId: $stateParams.entityId
                    }, function (data) {
                        vm.entity = data;
                        vm.project = vm.entity.project;
                    }, function (err) {
                        if (err.status === 404) {
                            notifications.error("The requested entity does not exist.", "Error");
                        }
                        else {
                            notifications.error("Failed to load the entity.", "Error", err);
                        }
                        $state.go("app.project", { projectId: $stateParams.projectId });
                    })
                        .$promise);
                    promises.push(relationshipResource.query({
                        pageSize: 0,
                        childEntityId: $stateParams.entityId
                    }, function (data) {
                        vm.relationshipsAsChild = data;
                    }, function (err) {
                        notifications.error("Failed to load the relationships.", "Error", err);
                        $state.go("app.project", { projectId: $stateParams.projectId });
                    }).$promise);
                    promises.push(fieldResource.query({
                        pageSize: 0,
                        entityId: $stateParams.entityId
                    }, function (data) {
                        vm.fields = data;
                    }, function (err) {
                        notifications.error("Failed to load the fields.", "Error", err);
                        $state.go("app.project", { projectId: $stateParams.projectId });
                    }).$promise);
                    promises.push(relationshipResource.query({
                        pageSize: 0,
                        parentEntityId: $stateParams.entityId
                    }, function (data) {
                        vm.relationshipsAsParent = data;
                    }, function (err) {
                        notifications.error("Failed to load the relationships.", "Error", err);
                        $state.go("app.project", { projectId: $stateParams.projectId });
                    }).$promise);
                    promises.push(codeReplacementResource.query({
                        pageSize: 0,
                        entityId: $stateParams.entityId
                    }, function (data) {
                        vm.codeReplacements = data;
                    }, function (err) {
                        notifications.error("Failed to load the codeReplacements.", "Error", err);
                        $state.go("app.project", { projectId: $stateParams.projectId });
                    }).$promise);
                    $q.all(promises).finally(function () { return vm.loading = false; });
                }
            });
        }
        function save() {
            if ($scope.mainForm.$invalid) {
                notifications.error("The form has not been completed correctly.", "Error");
            }
            else {
                vm.loading = true;
                vm.entity.$save(function (data) {
                    vm.entity = data;
                    notifications.success("The entity has been saved.", "Saved");
                    $state.go("app.entity", {
                        entityId: vm.entity.entityId
                    });
                }, function (err) {
                    errorService.handleApiError(err, "entity");
                }).finally(function () { return vm.loading = false; });
            }
        }
        ;
        function del() {
            if (confirm("Confirm delete?")) {
                vm.loading = true;
                entityResource.delete({
                    entityId: $stateParams.entityId
                }, function () {
                    notifications.success("The entity has been deleted.", "Deleted");
                    $state.go("app.project", { projectId: $stateParams.projectId });
                }, function (err) {
                    errorService.handleApiError(err, "entity", "delete");
                })
                    .$promise.finally(function () { return vm.loading = false; });
            }
        }
        function getSearchType(id) {
            return appSettings.searchType.filter(function (item) { return item.id === id; })[0].name;
        }
        function sortFields(e, ui) {
            var ids = [];
            angular.forEach(vm.fields, function (item, index) {
                ids.push(item.fieldId);
            });
            vm.loading = true;
            fieldResource.sort({
                ids: ids
            }, function (data) {
                notifications.success("The sort order has been updated", "Field Ordering");
            }, function (err) {
                notifications.error("Failed to sort the fields. " + (err.data && err.data.message ? err.data.message : ""), "Error", err);
            })
                .$promise.finally(function () { return vm.loading = false; });
        }
        function sortParentRelationships(e, ui) {
            var ids = [];
            angular.forEach(vm.relationshipsAsParent, function (item, index) {
                ids.push(item.relationshipId);
            });
            vm.loading = true;
            relationshipResource.sort({
                ids: ids
            }, function (data) {
                notifications.success("The sort order has been updated", "Parent Relationship Ordering");
            }, function (err) {
                notifications.error("Failed to sort the parent relationships. " + (err.data && err.data.message ? err.data.message : ""), "Error", err);
            })
                .$promise.finally(function () { return vm.loading = false; });
        }
        function sortCodeReplacements(e, ui) {
            var ids = [];
            angular.forEach(vm.codeReplacements, function (item, index) {
                ids.push(item.codeReplacementId);
            });
            vm.loading = true;
            codeReplacementResource.sort({
                ids: ids
            }, function (data) {
                notifications.success("The sort order has been updated", "Code Replacement Ordering");
            }, function (err) {
                notifications.error("Failed to sort the code replacements. " + (err.data && err.data.message ? err.data.message : ""), "Error", err);
            })
                .$promise.finally(function () { return vm.loading = false; });
        }
    }
    ;
}());
//# sourceMappingURL=entity.js.map