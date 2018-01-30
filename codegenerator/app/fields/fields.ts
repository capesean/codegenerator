/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";

    angular
        .module("app")
        .controller("fields", fields);

    fields.$inject = ["$scope", "$state", "$q", "$timeout", "notifications", "appSettings", "fieldResource"];
    function fields($scope, $state, $q, $timeout, notifications, appSettings, fieldResource) {

        var vm = this;
        vm.loading = true;
        vm.appSettings = appSettings;
        vm.search = { };
        vm.runSearch = runSearch;
        vm.goToField = (projectId, entityId, fieldId) => $state.go("app.field", { projectId: projectId, entityId: entityId, fieldId: fieldId });
        vm.sortOptions = { stop: sortItems, handle: "i.sortable-handle", axis: "y" };
        vm.moment = moment;

        initPage();

        function initPage() {

            var promises = [];

            $q.all(promises).finally(() => runSearch(0));

        }

        function runSearch(pageIndex) {

            vm.loading = true;

            var promises = [];

            promises.push(
                fieldResource.query(
                    {
                        q: vm.search.q,
                        includeEntities: true,
                        pageSize: 0
                    },
                    (data, headers) => {

                        vm.fields = data;

                    },
                    err => {

                        notifications.error("Failed to load the fields.", "Error", err);
                        $state.go("app.home");

                    }).$promise
            );

            $q.all(promises).finally(() => vm.loading = false);

        };

        function sortItems() {

            vm.loading = true;

            var ids = [];
            angular.forEach(vm.fields, function (item, index) {
                ids.push(item.fieldId);
            });

            fieldResource.sort(
                {
                    ids: ids
                },
                data => {

                    notifications.success("The sort order has been updated", "Field Ordering");

                },
                err => {

                    notifications.error("Failed to sort the fields. " + (err.data && err.data.message ? err.data.message : ""), "Error", err);

                })
                .$promise.finally(() => vm.loading = false);

        }

    };
} ());
