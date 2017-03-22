/// <reference path="../../scripts/typings/highlightjs/highlightjs.d.ts" />
/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";
    angular
        .module("app")
        .controller("entityCode", entityCode);
    entityCode.$inject = ["$scope", "$state", "$timeout", "$stateParams", "entityResource", "notifications", "appSettings", "$q", "errorService", "projectResource", "fieldResource", "clipboard"];
    function entityCode($scope, $state, $timeout, $stateParams, entityResource, notifications, appSettings, $q, errorService, projectResource, fieldResource, clipboard) {
        var vm = this;
        vm.deploymentOptions = {
            model: false,
            enums: false,
            dto: false,
            settingsDTO: false,
            dbContext: false,
            controller: false,
            bundleConfig: false,
            appRouter: false,
            apiResource: false,
            listHtml: false,
            listTypeScript: false,
            editHtml: false,
            editTypeScript: false
        };
        vm.loading = true;
        vm.getCode = getCode;
        vm.copy = copy;
        vm.deploy = deploy;
        vm.toggle = toggle;
        initPage();
        function initPage() {
            var promises = [];
            promises.push(entityResource.get({
                entityId: $stateParams.entityId
            }, function (data) {
                vm.entity = data;
                vm.project = vm.entity.project;
            }, function (err) {
                if (err.status === 404) {
                    notifications.error("The requested entity does not exist.", "Error");
                }
                else {
                    notifications.error("Failed to load the entity.", "Error", err);
                }
                vm.goToProject();
            }).$promise);
            $q.all(promises).finally(function () { return getCode(); });
        }
        function copy(item) {
            if (vm.code[item]) {
                clipboard.copyText(vm.code[item]);
                notifications.success("Copied " + item + " to clipboard", "Code Copied");
            }
            else {
                notifications.error("Invalid item: " + item, "Copy failed");
            }
        }
        function toggle() {
            var checkedCount = 0;
            var uncheckedCount = 0;
            for (var k in vm.deploymentOptions)
                if (vm.deploymentOptions.hasOwnProperty(k) && k.substring(0, 1) !== "$" && $scope.mainForm[k]) {
                    if (vm.deploymentOptions[k] === true)
                        checkedCount++;
                    else
                        uncheckedCount++;
                }
            for (var k in vm.deploymentOptions)
                if (vm.deploymentOptions.hasOwnProperty(k) && k.substring(0, 1) !== "$" && $scope.mainForm[k]) {
                    vm.deploymentOptions[k] = checkedCount < uncheckedCount;
                }
        }
        function htmlEscape(str) {
            return String(str)
                .replace(/&/g, '&amp;')
                .replace(/"/g, '&quot;')
                .replace(/'/g, '&#39;')
                .replace(/</g, '&lt;')
                .replace(/>/g, '&gt;');
        }
        function getCode() {
            vm.loading = true;
            vm.code = undefined;
            entityResource.getCode({ entityId: $stateParams.entityId }, function (data) {
                setCode(data);
                //toastr.success("Code has been regenerated");
            }, function (err) {
                errorService.handleApiError(err, "code", "generate");
            }).$promise.finally(function () {
                vm.loading = false;
            });
        }
        function setCode(data) {
            vm.code = data;
            var code = {};
            for (var k in data) {
                if (data.hasOwnProperty(k) && k.substring(0, 1) !== "$")
                    code[k] = htmlEscape(data[k]);
            }
            vm.escapedCode = code;
            $timeout(function () {
                $('pre code').each(function (i, block) {
                    hljs.highlightBlock(block);
                });
            }, 0);
        }
        function deploy() {
            if ($scope.mainForm.$invalid) {
                notifications.error("The form has not been completed correctly.", "Error");
            }
            else {
                vm.loading = true;
                var data = { entityId: $stateParams.entityId };
                var oneChecked = false;
                for (var k in vm.deploymentOptions)
                    if (vm.deploymentOptions.hasOwnProperty(k) && k.substring(0, 1) !== "$") {
                        data[k] = vm.deploymentOptions[k];
                        if (vm.deploymentOptions[k] === true)
                            oneChecked = true;
                    }
                if (!oneChecked) {
                    notifications.error("Nothing selected for deployment.", "Code Deployment");
                    vm.loading = false;
                    return;
                }
                entityResource.deploy(data, function (data) {
                    setCode(data);
                    notifications.success("Deployment was successful.", "Code Deployment");
                }, function (err) {
                    errorService.handleApiError(err, "code", "deploy");
                }).$promise.finally(function () { return vm.loading = false; });
            }
        }
    }
    ;
}());
//# sourceMappingURL=entitycode.js.map