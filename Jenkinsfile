pipeline {
    agent any
    
    parameters {
        choice(
            name: 'ENVIRONMENT',
            choices: ['Development', 'Pre', 'Live'],
            description: 'Select deployment environment'
        )
        string(
            name: 'BUILD_VERSION',
            defaultValue: '1.0.0',
            description: 'Build version number'
        )
    }
    
    environment {
        DOTNET_CLI_HOME = '/tmp/dotnet'
        SOLUTION_NAME = 'SampleAPI'
        PROJECT_NAME = 'SampleAPI'
        ASPNETCORE_ENVIRONMENT = "${params.ENVIRONMENT}"
    }
    
    stages {
        stage('Checkout') {
            steps {
                echo 'Checking out code from repository...'
                checkout scm
            }
        }
        
        stage('Restore Dependencies') {
            steps {
                echo 'Restoring NuGet packages...'
                sh '''
                    dotnet restore ${SOLUTION_NAME}.sln
                '''
            }
        }
        
        stage('Build') {
            steps {
                echo "Building solution for ${params.ENVIRONMENT} environment..."
                sh '''
                    dotnet build ${SOLUTION_NAME}.sln \
                        --configuration Release \
                        --no-restore \
                        /p:Version=${BUILD_VERSION}
                '''
            }
        }
        
        stage('Run Unit Tests') {
            steps {
                echo 'Running unit tests...'
                sh '''
                    dotnet test ${SOLUTION_NAME}.sln \
                        --configuration Release \
                        --no-build \
                        --verbosity normal \
                        --logger "trx;LogFileName=test-results.trx"
                '''
            }
            post {
                always {
                    // Publish test results
                    step([$class: 'MSTestPublisher', testResultsFile: '**/test-results.trx'])
                }
            }
        }
        
        stage('Publish') {
            steps {
                echo "Publishing application for ${params.ENVIRONMENT}..."
                sh '''
                    dotnet publish ${PROJECT_NAME}/${PROJECT_NAME}.csproj \
                        --configuration Release \
                        --no-build \
                        --output ./publish \
                        /p:EnvironmentName=${ASPNETCORE_ENVIRONMENT}
                '''
            }
        }
        
        stage('Package') {
            steps {
                echo 'Creating deployment package...'
                sh '''
                    cd publish
                    tar -czf ../${PROJECT_NAME}-${BUILD_VERSION}-${ASPNETCORE_ENVIRONMENT}.tar.gz *
                    cd ..
                '''
            }
        }
        
        stage('Deploy') {
            steps {
                script {
                    echo "Deploying to ${params.ENVIRONMENT} environment..."
                    
                    def deploymentConfig = getDeploymentConfig(params.ENVIRONMENT)
                    
                    sh """
                        # Copy package to deployment server
                        scp ${PROJECT_NAME}-${BUILD_VERSION}-${ASPNETCORE_ENVIRONMENT}.tar.gz \
                            ${deploymentConfig.user}@${deploymentConfig.host}:${deploymentConfig.path}
                        
                        # Extract and deploy
                        ssh ${deploymentConfig.user}@${deploymentConfig.host} << 'ENDSSH'
                            cd ${deploymentConfig.path}
                            
                            # Backup current version
                            if [ -d "current" ]; then
                                mv current backup_\$(date +%Y%m%d_%H%M%S)
                            fi
                            
                            # Extract new version
                            mkdir -p current
                            tar -xzf ${PROJECT_NAME}-${BUILD_VERSION}-${ASPNETCORE_ENVIRONMENT}.tar.gz -C current
                            
                            # Update environment configuration
                            export ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT}
                            
                            # Restart service
                            sudo systemctl restart ${PROJECT_NAME}
                            
                            # Health check
                            sleep 10
                            curl -f http://localhost:5000/health || exit 1
ENDSSH
                    """
                }
            }
        }
        
        stage('Smoke Tests') {
            steps {
                echo 'Running smoke tests...'
                script {
                    def deploymentConfig = getDeploymentConfig(params.ENVIRONMENT)
                    
                    sh """
                        # Basic health check
                        curl -f ${deploymentConfig.apiUrl}/health
                        
                        # API availability check
                        curl -f ${deploymentConfig.apiUrl}/api/v1/health
                    """
                }
            }
        }
    }
    
    post {
        success {
            echo "Deployment to ${params.ENVIRONMENT} completed successfully!"
            // Send success notification
            emailext(
                subject: "SUCCESS: Deployment to ${params.ENVIRONMENT}",
                body: "Build ${BUILD_VERSION} deployed successfully to ${params.ENVIRONMENT}",
                to: '${DEFAULT_RECIPIENTS}'
            )
        }
        failure {
            echo "Deployment to ${params.ENVIRONMENT} failed!"
            // Send failure notification
            emailext(
                subject: "FAILURE: Deployment to ${params.ENVIRONMENT}",
                body: "Build ${BUILD_VERSION} deployment to ${params.ENVIRONMENT} failed",
                to: '${DEFAULT_RECIPIENTS}'
            )
        }
        always {
            // Clean workspace
            cleanWs()
        }
    }
}

def getDeploymentConfig(environment) {
    def configs = [
        'Development': [
            host: 'dev-server.example.com',
            user: 'deploy',
            path: '/var/www/sampleapi/dev',
            apiUrl: 'https://dev-api.example.com'
        ],
        'Pre': [
            host: 'pre-server.example.com',
            user: 'deploy',
            path: '/var/www/sampleapi/pre',
            apiUrl: 'https://pre-api.example.com'
        ],
        'Live': [
            host: 'prod-server.example.com',
            user: 'deploy',
            path: '/var/www/sampleapi/prod',
            apiUrl: 'https://api.example.com'
        ]
    ]
    
    return configs[environment]
}
