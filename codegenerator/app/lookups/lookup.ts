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
        vm.goToLookupOption = (projectId, lookupId, lookupOptionId) => $state.go("app.lookupOption", { projectId: projectId, lookupId: lookupId, lookupOptionId: lookupOptionId });
        vm.loadLookupOptions = loadLookupOptions;
        vm.sortableOptions = {
            stop: updateOrders,
            handle: "i.sortable-handle"
        };

        initPage();
        function initPage() {

            var promises = [];

            $q.all(promises)
                .then(() => {

                    if (vm.isNew) {

                        vm.lookup = new lookupResource();
                        vm.lookup.lookupId = appSettings.newGuid;
                        vm.lookup.projectId = $stateParams.projectId;

                        promises = [];

                        promises.push(
                            projectResource.get(
                                {
                                    projectId: $stateParams.projectId
                                },
                                data => {
                                    vm.project = data;
                                },
                                err => {

                                    if (err.status === 404) {
                                        notifications.error("The requested project does not exist.", "Error");
                                    } else {
                                        notifications.error("Failed to load the project.", "Error", err);
                                    }

                                    $state.go("app.project", { projectId: $stateParams.projectId });

                                }).$promise
                        );

                        $q.all(promises).finally(() => vm.loading = false);

                    } else {

                        promises = [];

                        promises.push(
                            lookupResource.get(
                                {
                                    lookupId: $stateParams.lookupId
                                },
                                data => {
                                    vm.lookup = data;
                                    vm.project = vm.lookup.project;
                                },
                                err => {

                                    if (err.status === 404) {
                                        notifications.error("The requested lookup does not exist.", "Error");
                                    } else {
                                        notifications.error("Failed to load the lookup.", "Error", err);
                                    }

                                    $state.go("app.project", { projectId: $stateParams.projectId });

                                }).$promise
                        );

                        promises.push(loadLookupOptions(0, true));

                        $q.all(promises).finally(() => vm.loading = false);
                    }
                });
        }

        function save() {

            if ($scope.mainForm.$invalid) {

                notifications.error("The form has not been completed correctly.", "Error");

            } else {

                vm.loading = true;

                vm.lookup.$save(
                    data => {

                        notifications.success("The lookup has been saved.", "Saved");
                        if (vm.isNew)
                            $state.go("app.lookup", {
                                lookupId: vm.lookup.lookupId
                            });

                    },
                    err=> {

                        errorService.handleApiError(err, "lookup");

                    }).finally(() => vm.loading = false);

            }
        };

        function del() {

            if (confirm("Confirm delete?")) {

                vm.loading = true;

                lookupResource.delete(
                    {
                        lookupId: $stateParams.lookupId
                    },
                    () => {

                        notifications.success("The lookup has been deleted.", "Deleted");
                        $state.go("app.project", { projectId: $stateParams.projectId });

                    }, err => {

                        errorService.handleApiError(err, "lookup", "delete");

                    })
                    .$promise.finally(() => vm.loading = false);

            }
        }

        function loadLookupOptions(pageIndex, dontSetLoading) {

            if (!dontSetLoading) vm.loading = true;

            var promise = lookupOptionResource.query(
                {
                    lookupId: $stateParams.lookupId,
                    pageIndex: pageIndex,
                    includeEntities: true
                },
                (data, headers) => {
                    vm.lookupOptionsHeaders = JSON.parse(headers("X-Pagination"))
                    vm.lookupOptions = data;
                },
                err => {

                    notifications.error("Failed to load the lookup Options.", "Error", err);
                    $state.go("app.lookups");

                }).$promise;

            promise.finally(() => { if (!dontSetLoading) vm.loading = false; });

            return promise;
        }

        function updateOrders(e, ui) {

            var ids = [];
            angular.forEach(vm.lookupOptions, function (option, index) {
                ids.push(option.lookupOptionId);
            });

            vm.loading = true;
            lookupResource.updateOrders(
                {
                    lookupId: $stateParams.lookupId,
                    ids: ids
                },
                data => {

                    notifications.success("The order has been updated", "Update Order");

                },
                err => {

                    // todo: reset to original order by re-sorting the milestones manually
                    errorService.handleApiError(err, "fields", "re-order");

                })
                .$promise.finally(() => vm.loading = false);

        }
    };

} ());