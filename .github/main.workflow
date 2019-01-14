workflow "New workflow" {
  on = "push"
  resolves = ["bin"]
}

action "bin" {
  uses = "actions/bin/filter@b2bea0749eed6beb495a8fa194c071847af60ea1"
  args = "branch *"
}
