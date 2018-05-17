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
        vm.searchObjects = { };
        vm.runSearch = runSearch;
        vm.goToField = (projectId, entityId, fieldId) => $state.go("app.field", { projectId: projectId, entityId: entityId, fieldId: fieldId });
        vm.moment = moment;

        initPage();

        function initPage() {

            var promises = [];

            promises.push(runSearch(0, true));

            $q.all(promises).finally(() => vm.loading = false);

        }

        function runSearch(pageIndex, dontSetLoading) {

            if (!dontSetLoading) vm.loading = true;

            vm.search.includeEntities = true;
            vm.search.pageIndex = pageIndex;

            var promise =
                fieldResource.query(
                    vm.search,
                    (data, headers) => {

                        vm.fields = data;
                        vm.headers = JSON.parse(headers("X-Pagination"))

                    },
                    err => {

                        notifications.error("Failed to load the fields.", "Error", err);
                        $state.go("app.home");

                    }).$promise;

            if (!dontSetLoading) promise.then(() => vm.loading = false);

            return promise;

        };

    };
}());
