/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";
    angular
        .module("app")
        .controller("lookup", lookup);
    lookup.$inject = ["$scope", "$state", "$stateParams", "lookupResource", "notifications", "appSettings", "$q", "errorService", "projectResource", "lookupOptionResource"];
    function lookup($scope, $state, $stateParams, lookupResource, notifications, appSettings, $q, errorService, projectResource, lookupOptionResource) {
        var vm = this;
        vm.loading = true;
        vm.appSettings = appSettings;
        vm.user = null;
        vm.save = save;
        vm.delete = del;
        vm.isNew = $stateParams.lookupId === vm.appSettings.newGuid;
        vm.goToLookupOption = function (projectId, lookupId, lookupOptionId) { return $state.go("app.lookupOption", { projectId: projectId, lookupId: lookupId, lookupOptionId: lookupOptionId }); };
        vm.loadLookupOptions = loadLookupOptions;
        vm.sortableOptions = {
            stop: updateOrders,
            handle: "i.sortable-handle"
        };
        initPage();
        function initPage() {
            var promises = [];
            $q.all(promises)
                .then(function () {
                if (vm.isNew) {
                    vm.lookup = new lookupResource();
                    vm.lookup.lookupId = appSettings.newGuid;
                    vm.lookup.projectId = $stateParams.projectId;
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
                    }).$promise);
                    $q.all(promises).finally(function () { return vm.loading = false; });
                }
                else {
                    promises = [];
                    promises.push(lookupResource.get({
                        lookupId: $stateParams.lookupId
                    }, function (data) {
                        vm.lookup = data;
                        vm.project = vm.lookup.project;
                    }, function (err) {
                        if (err.status === 404) {
                            notifications.error("The requested lookup does not exist.", "Error");
                        }
                        else {
                            notifications.error("Failed to load the lookup.", "Error", err);
                        }
                        $state.go("app.project", { projectId: $stateParams.projectId });
                    }).$promise);
                    promises.push(loadLookupOptions(0, true));
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
                vm.lookup.$save(function (data) {
                    notifications.success("The lookup has been saved.", "Saved");
                    if (vm.isNew)
                        $state.go("app.lookup", {
                            lookupId: vm.lookup.lookupId
                        });
                }, function (err) {
                    errorService.handleApiError(err, "lookup");
                }).finally(function () { return vm.loading = false; });
            }
        }
        ;
        function del() {
            if (confirm("Confirm delete?")) {
                vm.loading = true;
                lookupResource.delete({
                    lookupId: $stateParams.lookupId
                }, function () {
                    notifications.success("The lookup has been deleted.", "Deleted");
                    $state.go("app.project", { projectId: $stateParams.projectId });
                }, function (err) {
                    errorService.handleApiError(err, "lookup", "delete");
                })
                    .$promise.finally(function () { return vm.loading = false; });
            }
        }
        function loadLookupOptions(pageIndex, dontSetLoading) {
            if (!dontSetLoading)
                vm.loading = true;
            var promise = lookupOptionResource.query({
                lookupId: $stateParams.lookupId,
                pageIndex: pageIndex,
                includeEntities: true
            }, function (data, headers) {
                vm.lookupOptionsHeaders = JSON.parse(headers("X-Pagination"));
                vm.lookupOptions = data;
            }, function (err) {
                notifications.error("Failed to load the lookup Options.", "Error", err);
                $state.go("app.lookups");
            }).$promise;
            promise.finally(function () { if (!dontSetLoading)
                vm.loading = false; });
            return promise;
        }
        function updateOrders(e, ui) {
            var ids = [];
            angular.forEach(vm.lookupOptions, function (option, index) {
                ids.push(option.lookupOptionId);
            });
            vm.loading = true;
            lookupResource.updateOrders({
                lookupId: $stateParams.lookupId,
                ids: ids
            }, function (data) {
                notifications.success("The order has been updated", "Update Order");
            }, function (err) {
                // todo: reset to original order by re-sorting the milestones manually
                errorService.handleApiError(err, "fields", "re-order");
            })
                .$promise.finally(function () { return vm.loading = false; });
        }
    }
    ;
}());
//# sourceMappingURL=lookup.js.map