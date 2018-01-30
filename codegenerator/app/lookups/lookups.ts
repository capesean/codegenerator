/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";

    angular
        .module("app")
        .controller("lookups", lookups);

    lookups.$inject = ["$scope", "$state", "$q", "$timeout", "notifications", "appSettings", "lookupResource"];
    function lookups($scope, $state, $q, $timeout, notifications, appSettings, lookupResource) {

        var vm = this;
        vm.loading = true;
        vm.appSettings = appSettings;
        vm.search = { };
        vm.runSearch = runSearch;
        vm.goToLookup = (projectId, lookupId) => $state.go("app.lookup", { projectId: projectId, lookupId: lookupId });
        vm.moment = moment;

        initPage();

        function initPage() {

            var promises = [];

            $q.all(promises).finally(() => runSearch(0))

        }

        function runSearch(pageIndex) {

            vm.loading = true;

            var promises = [];

            promises.push(
                lookupResource.query(
                    {
                        q: vm.search.q,
                        pageIndex: pageIndex
                    },
                    (data, headers) => {

                        vm.lookups = data;
                        vm.headers = JSON.parse(headers("X-Pagination"))

                    },
                    err => {

                        notifications.error("Failed to load the lookups.", "Error", err);
                        $state.go("app.home");

                    }).$promise
            );

            $q.all(promises).finally(() => vm.loading = false);

        };

    };
} ());
