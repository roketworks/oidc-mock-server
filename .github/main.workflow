workflow "Build" {
  on = "push"
  resolves = ["Push"]
}

action "Docker Registry" {
  uses = "actions/docker/login@c08a5fc9e0286844156fefff2c141072048141f6"
  secrets = ["DOCKER_USERNAME", "DOCKER_PASSWORD"]
  needs = ["Filter Master Branch"]
}

action "Build Image" {
  uses = "actions/docker/cli@c08a5fc9e0286844156fefff2c141072048141f6"
  needs = ["Docker Registry"]
  args = "build . -f ./src/Web.Host/Dockerfile -t oidc-mock-server"
}

action "Docker Tag" {
  uses = "actions/docker/tag@c08a5fc9e0286844156fefff2c141072048141f6"
  args = "oidc-server-mock mikemcfarland/oidc-server-mock"
  needs = ["Build Image"]
}

action "Push" {
  uses = "actions/docker/cli@c08a5fc9e0286844156fefff2c141072048141f6"
  needs = ["Docker Tag"]
  args = "push"
}

action "Filter Master Branch" {
  uses = "actions/bin/filter@b2bea0749eed6beb495a8fa194c071847af60ea1"
  args = "branch master"
}
