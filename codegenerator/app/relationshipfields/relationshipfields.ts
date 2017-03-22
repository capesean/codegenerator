/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";

    angular
        .module("app")
        .controller("relationshipFields", relationshipFields);

    relationshipFields.$inject = ["$scope", "$state", "$q", "$timeout", "notifications", "appSettings", "relationshipFieldResource"];
    function relationshipFields($scope, $state, $q, $timeout, notifications, appSettings, relationshipFieldResource) {

        var vm = this;
        vm.loading = true;
        vm.appSettings = appSettings;
        vm.search = { };
        vm.runSearch = runSearch;
        vm.goToRelationshipField = (projectId, entityId, relationshipId, relationshipFieldId) => $state.go("app.relationshipField", { projectId: projectId, entityId: entityId, relationshipId: relationshipId, relationshipFieldId: relationshipFieldId });
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
                relationshipFieldResource.query(
                    {
                        pageIndex: pageIndex
                    },
                    (data, headers) => {

                        vm.relationshipFields = data;
                        vm.headers = JSON.parse(headers("X-Pagination"))

                    },
                    err => {

                        notifications.error("Failed to load the relationship fields.", "Error", err);
                        $state.go("app.home"); 

                    }).$promise
            );

            $q.all(promises).finally(() => vm.loading = false)

        };

    };
} ());
