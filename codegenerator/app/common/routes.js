/// <reference path="../../scripts/typings/toastr/toastr.d.ts" />
/// <reference path="../../scripts/typings/moment/moment.d.ts" />
/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";
    angular.module('appRoutes', []).config(appRoutes);
    appRoutes.$inject = ["$stateProvider", "$locationProvider", "$urlRouterProvider"];
    function appRoutes($stateProvider, $locationProvider, $urlRouterProvider) {
        var version = "?v=2";
        $locationProvider.html5Mode(true);
        $urlRouterProvider.otherwise("/");
        $stateProvider
            .state("app", {
            abstract: true,
            template: "<div ui-view></div>",
            resolve: {
                load: ["appStarter", function (appStarter) {
                        var appStarterPromise = appStarter.start();
                        return appStarterPromise.then(function () {
                            $("body").removeClass("loading bg-faded");
                        });
                    }]
            }
        }).state("app.home", {
            url: "/",
            templateUrl: "/app/home/home.html" + version,
            controller: "home",
            controllerAs: "vm",
            data: { allowAny: true },
            ncyBreadcrumb: {
                label: "Home"
            }
        }).state("app.accessdenied", {
            url: "/accessdenied",
            templateUrl: "/app/login/denied.html" + version,
            data: { allowAny: true },
            ncyBreadcrumb: {
                skip: true
            }
        }).state("app.user", {
            url: "/users/:id",
            templateUrl: "/app/users/user.html",
            controller: "user",
            controllerAs: "vm",
            data: {
                roles: ["Administrator"]
            },
            ncyBreadcrumb: {
                parent: "app.users",
                label: "{{vm.user.firstName}} {{vm.user.lastName}}"
            }
        }).state("app.users", {
            url: "/users",
            templateUrl: "/app/users/users.html" + version,
            controller: "users",
            controllerAs: "vm",
            data: {
                roles: ["Administrator"]
            },
            ncyBreadcrumb: {
                label: "Users"
            }
        }).state("app.settings", {
            url: "/settings",
            templateUrl: "/app/settings/settings.html" + version,
            controller: "settings",
            controllerAs: "vm",
            data: {
                roles: ["Administrator"]
            },
            ncyBreadcrumb: {
                label: "Settings"
            }
        }).state("app.entityCode", {
            url: "/projects/:projectId/entities/:entityId/code",
            templateUrl: "/app/entities/entityCode.html",
            controller: "entityCode",
            controllerAs: "vm",
            data: {
                roles: ["Administrator"]
            },
            ncyBreadcrumb: {
                parent: "app.entity",
                label: "Code"
            }
        });
    }
})();
//# sourceMappingURL=routes.js.map