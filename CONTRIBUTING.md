# Rules for Contributing:

## Create Your Own Branch Before Working

**Do this before making any changes. Do not work directly on `main`.**

In Terminal, navigate to the `Corgi-Schnitzel` folder, then run:

```bash
git fetch 
git pull
git checkout -b yourname-work/feature
```

These commands will:

1. Fetch the latest updates from the remote repository.
2. Pull the latest changes so your local branch is up to date.
3. Create a new branch for your work and switch to it.

- Make changes on an individual development or feature branch, never directly on `main`.
- Use descriptive branch names, such as:
  - `feat/Corgi-length`
  - `fix/Collisions`
  - `docs/README`
- Keep branches and pull requests focused on a single change. Avoid unrelated cleanup.
- Discuss significant architectural changes in an issue before implementing them.
- Open a pull request when your focused change is ready for review.

### Confirm That You Are on Your Own Branch

Run:

```bash
git branch
```

The branch you are currently on will have an asterisk (`*`) next to it and it will probably be green as well.

**Make sure your own branch has the asterisk before you start working.**

> **Important:** Never make changes directly on `main`. Always work on your own development or feature branch.


