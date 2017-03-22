/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";

    angular
        .module("app")
        .controller("lookupOptions", lookupOptions);

    lookupOptions.$inject = ["$scope", "$state", "$q", "$timeout", "notifications", "appSettings", "lookupOptionResource"];
    function lookupOptions($scope, $state, $q, $timeout, notifications, appSettings, lookupOptionResource) {

        var vm = this;
        vm.loading = true;
        vm.appSettings = appSettings;
        vm.search = { };
        vm.runSearch = runSearch;
        vm.goToLookupOption = (projectId, lookupId, lookupOptionId) => $state.go("app.lookupOption", { projectId: projectId, lookupId: lookupId, lookupOptionId: lookupOptionId });
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
                lookupOptionResource.query(
                    {
                        friendlyName: vm.search.friendlyName,
                        name: vm.search.name,
                        pageIndex: pageIndex
                    },
                    (data, headers) => {

                        vm.lookupOptions = data;
                        vm.headers = JSON.parse(headers("X-Pagination"))

                    },
                    err => {

                        notifications.error("Failed to load the lookup options.", "Error", err);
                        $state.go("app.home"); 

                    }).$promise
            );

            $q.all(promises).finally(() => vm.loading = false)

        };

    };
} ());
