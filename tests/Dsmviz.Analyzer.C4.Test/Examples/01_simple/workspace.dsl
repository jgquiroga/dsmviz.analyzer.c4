workspace {

    model {
        user = person "User"
        
        softwareSystem = softwareSystem "Software System" {
            webapp1 = container "Web Application 1" {
            
                # Relationship from parent to this
                user -> this "Uses"
            }
            
            webapp2 = container "Web Application 2" {
                usersController = component "Users Controller"
                permissionsController = component "Permissions Controller"
            }
            
            webapp3 = container "Web Application 3"
            
            database1 = container "Database1" {
                webapp1 -> this "Reads from and writes to"
            }
            
            database2 = container "Database2"

            redis1 = container "Redis1"
        }

        group "Backoffice users" {
			admin = person "Admin"
		}
        
        # Relationship created outside
        user -> webapp2 "Uses"
        webapp2 -> database2 "Database"
        
        admin -> webapp3 "Uses"
        webapp3 -> database1 "Database"
        webapp3 -> database2 "Database"

        webapp1 -> usersController "Makes API calls to" "JSON/HTTPS"
        webapp1 -> permissionsController "Makes API calls to" "JSON/HTTPS"

        # Deployment nodes
        deploymentEnvironment = deploymentEnvironment "Development" {
            serviceInstance1 = deploymentGroup "Service instance 1"
            serviceInstance2 = deploymentGroup "Service instance 2"

			webServer1 = deploymentNode "Web Server 1" {
				webapp1Instance = containerInstance webapp1
				
				webapp2Instance = containerInstance webapp2

                deploymentNode "Redis Server" {
					redis1Instance = containerInstance redis1 serviceInstance1
                }
			}
			
			webServer2 = deploymentNode "Web Server 2" {
				webapp3Instance = containerInstance webapp3

                deploymentNode "Redis Server" {
					redis2Instance = containerInstance redis1 serviceInstance2
                }
			}
			
			databaseServer = deploymentNode "Database Server" {
				database1Instance = containerInstance database1
				database2Instance = containerInstance database2
			}
		}

        deploymentEnvironmentLive = deploymentEnvironment "Live" {
			deploymentNode "Amazon Web Services" {
                deploymentNode "US-East-1" {
                    route53 = infrastructureNode "Route 53"
                    elb = infrastructureNode "Elastic Load Balancer"

                    deploymentNode "Amazon EC2" {
						deploymentNode "Ubuntu Server" {
							webapp1InstanceLive = containerInstance webapp1
							redis1InstanceLive = containerInstance redis1
						}
					}
                }
            }

            route53 -> elb "Forwards requests to" "HTTPS"
            elb -> webapp1InstanceLive "Forwards requests to" "HTTPS"
        }
    }

    views {
        systemContext softwareSystem {
            include *
            autolayout lr
        }

        container softwareSystem {
            include *
            autolayout lr
        }

        component webapp1 {
			include *
			autolayout lr
		}
		
		component webapp2 {
			include *
			autolayout lr
		}
		
		component webapp3 {
			include *
			autolayout lr
		}

        theme default
    }

}