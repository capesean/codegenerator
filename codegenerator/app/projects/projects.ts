/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";

    angular
        .module("app")
        .controller("projects", projects);

    projects.$inject = ["$scope", "$state", "$q", "$timeout", "notifications", "appSettings", "projectResource"];
    function projects($scope, $state, $q, $timeout, notifications, appSettings, projectResource) {

        var vm = this;
        vm.loading = true;
        vm.appSettings = appSettings;
        vm.search = { };
        vm.runSearch = runSearch;
        vm.goToProject = (projectId) => $state.go("app.project", { projectId: projectId });
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
                projectResource.query(
                    {
                        q: vm.search.q,
                        pageIndex: pageIndex
                    },
                    (data, headers) => {

                        vm.projects = data;
                        vm.headers = JSON.parse(headers("X-Pagination"))

                    },
                    err => {

                        notifications.error("Failed to load the projects.", "Error", err);
                        $state.go("app.home");

                    }).$promise
            );

            $q.all(promises).finally(() => vm.loading = false)

        };

    };
} ());
