// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";

    angular
        .module("app")
        .controller("selectNAMEModal", selectNAMEModal);

    selectNAMEModal.$inject = ["$scope", "appSettings", "$uibModalInstance", "notifications", "$q", "NAME_CAMELResource", "options"];
    function selectNAMEModal($scope, appSettings, $uibModalInstance, notifications, $q, NAME_CAMELResource, options) {

        var vm = this;
        vm.loading = true;
        vm.appSettings = appSettings;
        vm.select = select;
        vm.cancel = cancel;
        vm.search = {};
        vm.runNAMESearch = runNAMESearch;
        vm.options = options;
        vm.selectedItems = options.PLURALNAME_LOWER ? angular.copy(options.PLURALNAME_LOWER) : [];
        vm.close = close;
        vm.clear = clear;
        vm.isSelected = isSelected;
        vm.selectAll = selectAll;
        vm.options = options;

        init();

        function init() {

            options.singular = options.singular || "NAME";
            options.plural = options.plural || "PLURALNAME";

            vm.search = {
                includeEntities: true
            };

            runNAMESearch(0, false);

        }

        function runNAMESearch(pageIndex, dontSetLoading) {

            if (!dontSetLoading) vm.loading = true;
            vm.search.pageIndex = pageIndex;

            var promise =
                NAME_CAMELResource.query(
                    vm.search,
                    (data, headers) => {

                        vm.PLURALNAME_LOWER = data;
                        vm.NAME_LOWERHeaders = JSON.parse(headers("X-Pagination"))

                    },
                    err => {

                        notifications.error("Failed to load the PLURALNAME_LOWER.", "Error", err);
                        vm.cancel();

                    }).$promise;

            if (!dontSetLoading) promise.then(() => vm.loading = false);

            return promise;

        };

        function cancel() {
            $uibModalInstance.dismiss();
        }

        function close() {
            if (!!options.multiple) $uibModalInstance.close(vm.selectedItems);
            else $uibModalInstance.dismiss();
        }

        function clear() {
            $uibModalInstance.close(undefined);
        }

        function select(NAME_LOWER) {
            if (!!options.multiple) {
                if (isSelected(NAME_LOWER)) {
                    for (var i = 0; i < vm.selectedItems.length; i++) {
                        if (vm.selectedItems[i].NAME_LOWERId == NAME_LOWER.NAME_LOWERId) {
                            vm.selectedItems.splice(i, 1);
                            break;
                        }
                    }
                } else {
                    vm.selectedItems.push(NAME_LOWER);
                }
            } else {
                $uibModalInstance.close(NAME_LOWER);
            }
        }

        function isSelected(NAME_LOWER) {
            return vm.selectedItems.filter(item => item.NAME_LOWERId === NAME_LOWER.NAME_LOWERId).length > 0;
        }

        function selectAll() {

            vm.loading = true;

            NAME_CAMELResource.query(
                {
                    pageSize: 0
                },
                data => {

                    $uibModalInstance.close(data);

                },
                err => {

                    notifications.error("Failed to load the PLURALNAME_LOWER.", "Error", err);


                }).$promise.then(() => vm.loading = false);
        }
    }

}());
