/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";

    angular
        .module("app")
        .controller("relationships", relationships);

    relationships.$inject = ["$scope", "$state", "$q", "$timeout", "notifications", "appSettings", "relationshipResource", "entityResource"];
    function relationships($scope, $state, $q, $timeout, notifications, appSettings, relationshipResource, entityResource) {

        var vm = this;
        vm.loading = true;
        vm.appSettings = appSettings;
        vm.search = { };
        vm.runSearch = runSearch;
        vm.goToRelationship = (projectId, entityId, relationshipId) => $state.go("app.relationship", { projectId: projectId, entityId: entityId, relationshipId: relationshipId });
        vm.sortOptions = { stop: sortItems, handle: "i.sortable-handle", axis: "y" };
        vm.moment = moment;

        initPage();

        function initPage() {

            var promises = [];

            promises.push(
                entityResource.query(
                    {
                        pageSize: 0
                    },
                    (data, headers) => {

                        vm.entities = data;

                    },
                    err => {

                        notifications.error("Failed to load the entities.", "Error", err);
                        $state.go("app.home");

                    }).$promise
            );

            $q.all(promises).finally(() => runSearch(0))

        }

        function runSearch(pageIndex) {

            vm.loading = true;

            var promises = [];

            promises.push(
                relationshipResource.query(
                    {
                        parentEntityId: vm.search.parentEntityId,
                        childEntityId: vm.search.childEntityId,
                        pageSize: 0
                    },
                    (data, headers) => {

                        vm.relationships = data;

                    },
                    err => {

                        notifications.error("Failed to load the relationships.", "Error", err);
                        $state.go("app.home");

                    }).$promise
            );

            $q.all(promises).finally(() => vm.loading = false)

        };

        function sortItems() {

            vm.loading = true;

            var ids = [];
            angular.forEach(vm.relationships, function (item, index) {
                ids.push(item.relationshipId);
            });

            relationshipResource.sort(
                {
                    ids: ids
                },
                data => {

                    notifications.success("The sort order has been updated", "Relationship Ordering");

                },
                err => {

                    notifications.error("Failed to sort the relationships. " + (err.data && err.data.message ? err.data.message : ""), "Error", err);

                })
                .$promise.finally(() => vm.loading = false);

        }

    };
} ());
