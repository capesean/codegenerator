/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";
    angular
        .module("app")
        .controller("codeReplacement", codeReplacement);
    codeReplacement.$inject = ["$scope", "$state", "$stateParams", "codeReplacementResource", "notifications", "appSettings", "$q", "errorService", "entityResource"];
    function codeReplacement($scope, $state, $stateParams, codeReplacementResource, notifications, appSettings, $q, errorService, entityResource) {
        var vm = this;
        vm.loading = true;
        vm.appSettings = appSettings;
        vm.user = null;
        vm.save = save;
        vm.delete = del;
        vm.isNew = $stateParams.codeReplacementId === vm.appSettings.newGuid;
        initPage();
        function initPage() {
            var promises = [];
            $q.all(promises)
                .then(function () {
                if (vm.isNew) {
                    vm.codeReplacement = new codeReplacementResource();
                    vm.codeReplacement.codeReplacementId = appSettings.newGuid;
                    vm.codeReplacement.entityId = $stateParams.entityId;
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
                    promises.push(codeReplacementResource.get({
                        codeReplacementId: $stateParams.codeReplacementId
                    }, function (data) {
                        vm.codeReplacement = data;
                        vm.entity = vm.codeReplacement.entity;
                        vm.project = vm.entity.project;
                    }, function (err) {
                        if (err.status === 404) {
                            notifications.error("The requested code replacement does not exist.", "Error");
                        }
                        else {
                            notifications.error("Failed to load the code replacement.", "Error", err);
                        }
                        $state.go("app.entity", { projectId: $stateParams.projectId, entityId: $stateParams.entityId });
                    })
                        .$promise);
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
                vm.codeReplacement.$save(function (data) {
                    vm.codeReplacement = data;
                    notifications.success("The code replacement has been saved.", "Saved");
                    $state.go("app.entity", { projectId: $stateParams.projectId, entityId: $stateParams.entityId });
                }, function (err) {
                    errorService.handleApiError(err, "code replacement");
                }).finally(function () { return vm.loading = false; });
            }
        }
        ;
        function del() {
            if (confirm("Confirm delete?")) {
                vm.loading = true;
                codeReplacementResource.delete({
                    codeReplacementId: $stateParams.codeReplacementId
                }, function () {
                    notifications.success("The code replacement has been deleted.", "Deleted");
                    $state.go("app.entity", { projectId: $stateParams.projectId, entityId: $stateParams.entityId });
                }, function (err) {
                    errorService.handleApiError(err, "code replacement", "delete");
                })
                    .$promise.finally(function () { return vm.loading = false; });
            }
        }
    }
    ;
}());
//# sourceMappingURL=codereplacement.js.map