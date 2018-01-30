/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";

    angular
        .module("app")
        .controller("field", field);

    field.$inject = ["$scope", "$state", "$stateParams", "fieldResource", "notifications", "appSettings", "$q", "errorService", "entityResource", "lookupResource"];
    function field($scope, $state, $stateParams, fieldResource, notifications, appSettings, $q, errorService, entityResource, lookupResource) {

        var vm = this;
        vm.loading = true;
        vm.appSettings = appSettings;
        vm.user = null;
        vm.save = save;
        vm.delete = del;
        vm.isNew = $stateParams.fieldId === vm.appSettings.newGuid;
        vm.roles = [];
        vm.goToEntity = () => $state.go("app.entity", { projectId: $stateParams.projectId, entityId: $stateParams.entityId });
        vm.fieldType = appSettings.fieldType;
        vm.searchType = appSettings.searchType;
        vm.showLength = () => {
            if (!vm.field || vm.field.fieldType === undefined) return null;
            var x = $.grep(appSettings.fieldType, (type: any) => { return type.id === vm.field.fieldType; })[0];
            return x.name.toLowerCase().indexOf("varchar") > -1 || x.name.toLowerCase().indexOf("text") > -1;
        };
        vm.showPrecisionAndScale = () => {
            if (!vm.field || vm.field.fieldType === undefined) return null;
            var x = $.grep(appSettings.fieldType, (type: any) => { return type.id === vm.field.fieldType; })[0];
            return x.name.toLowerCase() === "decimal";
        };

        initPage();

        function initPage() {

            var promises = [];

            promises.push(
                lookupResource.query(
                    {
                        pageSize: 0,
                        projectId: $stateParams.projectId
                    },
                    data => {
                        vm.lookups = data;
                    },
                    err => {
                        notifications.error("Failed to load the lookups.", "Error", err);
                        $state.go("app.entity", { projectId: $stateParams.projectId, entityId: $stateParams.entityId });
                    }).$promise
            );

            $q.all(promises)
                .then(() => {

                    if (vm.isNew) {

                        vm.field = new fieldResource();
                        vm.field.fieldId = appSettings.newGuid;
                        vm.field.scale = 0;
                        vm.field.searchType = 0;
                        vm.field.precision = 0;
                        vm.field.length = 0;
                        vm.field.editPageType = 0;
                        vm.field.entityId = $stateParams.entityId;

                        promises = [];

                        promises.push(
                            entityResource.get(
                                {
                                    entityId: $stateParams.entityId
                                },
                                data => {
                                    vm.entity = data;
                                    vm.project = vm.entity.project;
                                },
                                err => {

                                    errorService.handleApiError(err, "entity", "load");
                                    $state.go("app.entity", { projectId: $stateParams.projectId, entityId: $stateParams.entityId });

                                }).$promise
                        );

                        $q.all(promises).finally(() => vm.loading = false);

                    } else {

                        promises = [];

                        promises.push(
                            fieldResource.get(
                                {
                                    fieldId: $stateParams.fieldId
                                },
                                data => {
                                    vm.field = data;
                                    vm.entity = vm.field.entity;
                                    vm.project = vm.entity.project;
                                },
                                err => {

                                    errorService.handleApiError(err, "field", "load");
                                    $state.go("app.entity", { projectId: $stateParams.projectId, entityId: $stateParams.entityId });

                                }).$promise
                        );

                        $q.all(promises).finally(() => vm.loading = false);
                    }
                });
        }

        function save() {

            if ($scope.mainForm.$invalid) {

                notifications.error("The form has not been completed correctly.", "Error");
                return;

            }

            vm.loading = true;

            vm.field.$save(
                data => {

                    notifications.success("The field has been saved.", "Saved");
                    if (vm.isNew)
                        $state.go("app.field", {
                            fieldId: vm.field.fieldId
                        });

                },
                err=> {

                    errorService.handleApiError(err, "field");

                }).finally(() => vm.loading = false);

        };

        function del() {

            if (!confirm("Confirm delete?")) return;

            vm.loading = true;

            fieldResource.delete(
                {
                    fieldId: $stateParams.fieldId
                },
                () => {

                    notifications.success("The field has been deleted.", "Deleted");
                    $state.go("app.entity", { projectId: $stateParams.projectId, entityId: $stateParams.entityId });

                }, err => {

                    errorService.handleApiError(err, "field", "delete");

                })
                .$promise.finally(() => vm.loading = false);

        }
    };

} ());
