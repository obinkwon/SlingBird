fastlane documentation
----

# Installation

Make sure you have the latest version of the Xcode command line tools installed:

```sh
xcode-select --install
```

For _fastlane_ installation instructions, see [Installing _fastlane_](https://docs.fastlane.tools/#installing-fastlane)

# Available Actions

## Android

### android build

```sh
[bundle exec] fastlane android build
```

AAB만 빌드 (build/android/SlingBird.aab)

### android internal

```sh
[bundle exec] fastlane android internal
```

AAB 빌드 후 Google Play 내부 테스트(internal) 트랙에 업로드

### android beta

```sh
[bundle exec] fastlane android beta
```

beta 배포

### android production

```sh
[bundle exec] fastlane android production
```

Production 배포

----

This README.md is auto-generated and will be re-generated every time [_fastlane_](https://fastlane.tools) is run.

More information about _fastlane_ can be found on [fastlane.tools](https://fastlane.tools).

The documentation of _fastlane_ can be found on [docs.fastlane.tools](https://docs.fastlane.tools).
