/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";

    angular
        .module("app")
        .controller("relationshipField", relationshipField);

    relationshipField.$inject = ["$scope", "$state", "$stateParams", "relationshipFieldResource", "notifications", "appSettings", "$q", "errorService", "relationshipResource", "fieldResource"];
    function relationshipField($scope, $state, $stateParams, relationshipFieldResource, notifications, appSettings, $q, errorService, relationshipResource, fieldResource) {

        var vm = this;
        vm.loading = true;
        vm.user = null;
        vm.save = save;
        vm.delete = del;
        vm.isNew = $stateParams.relationshipFieldId === appSettings.newGuid;

        initPage();

        function initPage() {

            var promises = [

                relationshipResource.get(
                    {
                        relationshipId: $stateParams.relationshipId
                    },
                    data => {

                        vm.relationship = data;
                        vm.entity = vm.relationship.parentEntity;
                        vm.childEntity = vm.relationship.childEntity;
                        vm.project = vm.entity.project;

                        promises.push(
                            fieldResource.query(
                                {
                                    pageSize: 0,
                                    entityId: vm.relationship.parentEntityId
                                },
                                data => {
                                    vm.parentFields = data;
                                },
                                err => {

                                    notifications.error("Failed to load the parent fields list.", "Error", err);
                                    $state.go("app.relationships");

                                }
                            ).$promise
                        );

                        promises.push(
                            fieldResource.query(
                                {
                                    pageSize: 0,
                                    entityId: vm.relationship.childEntityId
                                },
                                data => {
                                    vm.childFields = data;
                                },
                                err => {

                                    notifications.error("Failed to load the child fields list.", "Error", err);
                                    $state.go("app.relationships");

                                }
                            ).$promise
                        );

                    },
                    err => {

                        notifications.error("Failed to load the relationship.", "Error", err);
                        $state.go("app.relationships");

                    }).$promise
            ];

            $q.all(promises)
                .then(() => {

                    if (vm.isNew) {

                        vm.relationshipField = new relationshipFieldResource();
                        vm.relationshipField.relationshipId = $stateParams.relationshipId;
                        vm.loading = false;

                    } else {

                        var promises = [];

                        promises.push(
                            relationshipFieldResource.get(
                                {
                                    relationshipFieldId: $stateParams.relationshipFieldId
                                },
                                data => {
                                    vm.relationshipField = data;
                                },
                                err => {

                                    if (err.status === 404) {
                                        notifications.error("The requested relationship field does not exist.", "Error");
                                    } else {
                                        notifications.error("Failed to load the relationship field.", "Error", err);
                                    }

                                    $state.go("app.relationshipFields");

                                })
                                .$promise);

                        $q.all(promises).finally(() => vm.loading = false);
                    }

                });
        }

        function save() {

            if ($scope.mainForm.$invalid) {

                notifications.error("The form has not been completed correctly.", "Error");

            } else {

                vm.loading = true;

                vm.relationshipField.$save(
                    data => {

                        vm.relationshipField = data;
                        notifications.success("The relationship field has been saved.", "Saved");
                        $state.go("app.relationship", { projectId: $stateParams.projectId, entityId: $stateParams.entityId, relationshipId: $stateParams.relationshipId });

                    },
                    err=> {

                        errorService.handleApiError(err, "relationship field");

                    }).finally(() => vm.loading = false);

            }
        };

        function del() {

            if (confirm("Confirm delete?")) {

                vm.loading = true;

                relationshipFieldResource.delete(
                    {
                        relationshipFieldId: $stateParams.relationshipFieldId
                    },
                    () => {

                        notifications.success("The relationship field has been deleted.", "Deleted");
                        $state.go("app.relationship", { projectId: $stateParams.projectId, entityId: $stateParams.entityId, relationshipId: $stateParams.relationshipId });

                    }, err => {

                        errorService.handleApiError(err, "relationship field", "delete");

                    })
                    .$promise.finally(() => vm.loading = false);

            }
        }
    };

} ());
