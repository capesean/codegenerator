/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";
    angular
        .module("app")
        .controller("relationship", relationship);
    relationship.$inject = ["$scope", "$state", "$stateParams", "relationshipResource", "notifications", "appSettings", "$q", "errorService", "entityResource", "fieldResource", "relationshipFieldResource"];
    function relationship($scope, $state, $stateParams, relationshipResource, notifications, appSettings, $q, errorService, entityResource, fieldResource, relationshipFieldResource) {
        var vm = this;
        vm.loading = true;
        vm.appSettings = appSettings;
        vm.user = null;
        vm.save = save;
        vm.delete = del;
        vm.isNew = $stateParams.relationshipId === vm.appSettings.newGuid;
        vm.goToRelationshipField = function (projectId, entityId, relationshipId, relationshipFieldId) { return $state.go("app.relationshipField", { projectId: $stateParams.projectId, entityId: $stateParams.entityId, relationshipId: relationshipId, relationshipFieldId: relationshipFieldId }); };
        vm.loadRelationshipFields = loadRelationshipFields;
        initPage();
        function initPage() {
            var promises = [];
            promises.push(entityResource.query({
                pageSize: 0,
                projectId: $stateParams.projectId
            }, function (data) {
                // remove current item & store in entity object
                //for (var i = data.length - 1; i >= 0; i--) {
                //    if (data[i].entityId === $stateParams.entityId) {
                //        vm.entity = data[i];
                //        vm.project = vm.entity.project;
                //        data.splice(i, 1);
                //    }
                //}
                vm.entities = data;
            }, function (err) {
                notifications.error("Failed to load the entities.", "Error", err);
                $state.go("app.relationships");
            }).$promise);
            promises.push(fieldResource.query({
                pageSize: 0,
                entityId: $stateParams.entityId
            }, function (data) {
                vm.fields = data;
            }, function (err) {
                notifications.error("Failed to load the fields.", "Error", err);
                $state.go("app.entity", { projectId: $stateParams.projectId, entityId: $stateParams.entityId });
            }).$promise);
            $q.all(promises)
                .then(function () {
                if (vm.isNew) {
                    vm.relationship = new relationshipResource();
                    vm.relationship.relationshipId = appSettings.newGuid;
                    vm.relationship.relationshipAncestorLimit = 1;
                    vm.relationship.parentEntityId = $stateParams.entityId;
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
                        $state.go("app.entity", { projectId: $stateParams.projectId, entityId: $stateParams.entityId });
                    })
                        .$promise);
                    $q.all(promises).finally(function () { return vm.loading = false; });
                }
                else {
                    promises = [];
                    promises.push(relationshipResource.get({
                        relationshipId: $stateParams.relationshipId
                    }, function (data) {
                        vm.relationship = data;
                        vm.entity = vm.relationship.parentEntity;
                        vm.project = vm.entity.project;
                    }, function (err) {
                        if (err.status === 404) {
                            notifications.error("The requested relationship does not exist.", "Error");
                        }
                        else {
                            notifications.error("Failed to load the relationship.", "Error", err);
                        }
                        $state.go("app.entity", { projectId: $stateParams.projectId, entityId: $stateParams.entityId });
                    })
                        .$promise);
                    promises.push(loadRelationshipFields(0, true));
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
                vm.relationship.$save(function (data) {
                    notifications.success("The relationship has been saved.", "Saved");
                    if (vm.isNew)
                        $state.go("app.relationship", {
                            relationshipId: vm.relationship.relationshipId
                        });
                }, function (err) {
                    errorService.handleApiError(err, "relationship");
                }).finally(function () { return vm.loading = false; });
            }
        }
        ;
        function del() {
            if (confirm("Confirm delete?")) {
                vm.loading = true;
                relationshipResource.delete({
                    relationshipId: $stateParams.relationshipId
                }, function () {
                    notifications.success("The relationship has been deleted.", "Deleted");
                    $state.go("app.entity", { projectId: $stateParams.projectId, entityId: $stateParams.entityId });
                }, function (err) {
                    errorService.handleApiError(err, "relationship", "delete");
                })
                    .$promise.finally(function () { return vm.loading = false; });
            }
        }
        function loadRelationshipFields(pageIndex, dontSetLoading) {
            if (!dontSetLoading)
                vm.loading = true;
            var promise = relationshipFieldResource.query({
                relationshipId: $stateParams.relationshipId,
                pageIndex: pageIndex
            }, function (data, headers) {
                vm.relationshipFieldsHeaders = JSON.parse(headers("X-Pagination"));
                vm.relationshipFields = data;
            }, function (err) {
                notifications.error("Failed to load the relationship Fields.", "Error", err);
                $state.go("app.relationships");
            }).$promise;
            promise.finally(function () { if (!dontSetLoading)
                vm.loading = false; });
            return promise;
        }
    }
    ;
}());
//# sourceMappingURL=relationship.js.map