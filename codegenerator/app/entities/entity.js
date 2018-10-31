/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";
    angular
        .module("app")
        .controller("entity", entity);
    entity.$inject = ["$scope", "$state", "$stateParams", "notifications", "appSettings", "$q", "errorService", "entityResource", "projectResource", "fieldResource", "relationshipResource", "codeReplacementResource"];
    function entity($scope, $state, $stateParams, notifications, appSettings, $q, errorService, entityResource, projectResource, fieldResource, relationshipResource, codeReplacementResource) {
        var vm = this;
        vm.loading = true;
        vm.appSettings = appSettings;
        vm.save = save;
        vm.delete = del;
        vm.isNew = $stateParams.entityId === vm.appSettings.newGuid;
        vm.goToRelationship = function (projectId, entityId, relationshipId) { return $state.go("app.relationship", { projectId: projectId, entityId: entityId, relationshipId: relationshipId }); };
        vm.loadRelationshipsAsChild = loadRelationshipsAsChild;
        vm.goToField = function (projectId, entityId, fieldId) { return $state.go("app.field", { projectId: projectId, entityId: entityId, fieldId: fieldId }); };
        vm.loadFields = loadFields;
        vm.fieldsSortOptions = { stop: sortFields, handle: "i.sortable-handle", axis: "y" };
        vm.goToRelationship = function (projectId, entityId, relationshipId) { return $state.go("app.relationship", { projectId: projectId, entityId: entityId, relationshipId: relationshipId }); };
        vm.loadRelationshipsAsParent = loadRelationshipsAsParent;
        vm.relationshipsSortOptions = { stop: sortRelationships, handle: "i.sortable-handle", axis: "y" };
        vm.goToCodeReplacement = function (projectId, entityId, codeReplacementId) { return $state.go("app.codeReplacement", { projectId: projectId, entityId: entityId, codeReplacementId: codeReplacementId }); };
        vm.loadCodeReplacements = loadCodeReplacements;
        vm.codeReplacementsSortOptions = { stop: sortCodeReplacements, handle: "i.sortable-handle", axis: "y" };
        initPage();
        function initPage() {
            var promises = [];
            promises.push(fieldResource.query({
                pageSize: 0
            }, function (data) {
                vm.fields = data;
            }, function (err) {
                notifications.error("Failed to load the fields.", "Error", err);
                $state.go("app.project", { projectId: $stateParams.projectId });
            }).$promise);
            if (vm.isNew) {
                vm.entity = new entityResource();
                vm.entity.entityId = appSettings.newGuid;
                vm.entity.entityType = 0;
                vm.entity.projectId = $stateParams.projectId;
                promises.push(projectResource.get({
                    projectId: $stateParams.projectId
                }, function (data) {
                    vm.project = data;
                }, function (err) {
                    errorService.handleApiError(err, "project", "load");
                    $state.go("app.project", { projectId: $stateParams.projectId });
                }).$promise);
            }
            else {
                promises.push(entityResource.get({
                    entityId: $stateParams.entityId
                }, function (data) {
                    vm.entity = data;
                    vm.project = vm.entity.project;
                }, function (err) {
                    errorService.handleApiError(err, "entity", "load");
                    $state.go("app.project", { projectId: $stateParams.projectId });
                }).$promise);
                promises.push(loadRelationshipsAsChild(0, true));
                promises.push(loadFields(0, true));
                promises.push(loadRelationshipsAsParent(0, true));
                promises.push(loadCodeReplacements(0, true));
            }
            $q.all(promises).finally(function () { return vm.loading = false; });
        }
        function save() {
            if ($scope.mainForm.$invalid) {
                notifications.error("The form has not been completed correctly.", "Error");
                return;
            }
            vm.loading = true;
            vm.entity.$save(function (data) {
                notifications.success("The entity has been saved.", "Saved");
                if (vm.isNew)
                    $state.go("app.entity", {
                        entityId: vm.entity.entityId
                    });
            }, function (err) {
                errorService.handleApiError(err, "entity");
            }).finally(function () { return vm.loading = false; });
        }
        function del() {
            if (!confirm("Confirm delete?"))
                return;
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
        function loadRelationshipsAsChild(pageIndex, dontSetLoading) {
            if (!dontSetLoading)
                vm.loading = true;
            var promise = relationshipResource.query({
                childEntityId: $stateParams.entityId,
                pageSize: 0,
                pageIndex: pageIndex
            }, function (data, headers) {
                vm.relationshipsAsChild = data;
            }, function (err) {
                notifications.error("Failed to load the relationships.", "Error", err);
                $state.go("app.entities");
            }).$promise;
            promise.finally(function () { if (!dontSetLoading)
                vm.loading = false; });
            return promise;
        }
        function loadFields(pageIndex, dontSetLoading) {
            if (!dontSetLoading)
                vm.loading = true;
            var promise = fieldResource.query({
                entityId: $stateParams.entityId,
                pageSize: 0,
                pageIndex: pageIndex,
                includeEntities: true
            }, function (data, headers) {
                vm.fields = data;
            }, function (err) {
                notifications.error("Failed to load the fields.", "Error", err);
                $state.go("app.entities");
            }).$promise;
            promise.finally(function () { if (!dontSetLoading)
                vm.loading = false; });
            return promise;
        }
        function loadRelationshipsAsParent(pageIndex, dontSetLoading) {
            if (!dontSetLoading)
                vm.loading = true;
            var promise = relationshipResource.query({
                parentEntityId: $stateParams.entityId,
                pageSize: 0,
                pageIndex: pageIndex,
                includeEntities: true
            }, function (data, headers) {
                vm.relationshipsAsParent = data;
            }, function (err) {
                notifications.error("Failed to load the relationships.", "Error", err);
                $state.go("app.entities");
            }).$promise;
            promise.finally(function () { if (!dontSetLoading)
                vm.loading = false; });
            return promise;
        }
        function loadCodeReplacements(pageIndex, dontSetLoading) {
            if (!dontSetLoading)
                vm.loading = true;
            var promise = codeReplacementResource.query({
                entityId: $stateParams.entityId,
                pageSize: 0,
                pageIndex: pageIndex,
                includeEntities: true
            }, function (data, headers) {
                vm.codeReplacements = data;
            }, function (err) {
                notifications.error("Failed to load the code Replacements.", "Error", err);
                $state.go("app.entities");
            }).$promise;
            promise.finally(function () { if (!dontSetLoading)
                vm.loading = false; });
            return promise;
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
        function sortRelationships(e, ui) {
            var ids = [];
            angular.forEach(vm.relationships, function (item, index) {
                ids.push(item.relationshipId);
            });
            vm.loading = true;
            relationshipResource.sort({
                ids: ids
            }, function (data) {
                notifications.success("The sort order has been updated", "Relationship Ordering");
            }, function (err) {
                notifications.error("Failed to sort the relationships. " + (err.data && err.data.message ? err.data.message : ""), "Error", err);
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