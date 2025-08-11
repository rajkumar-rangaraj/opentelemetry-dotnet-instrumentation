FROM quay.io/centos/centos:stream9@sha256:c8934ae041d271981c56152423f8eb694be316f2de53ec0924555f0a424eff7d

# Install dotnet sdk
RUN dnf install -y \
    libicu-devel

COPY ./scripts/dotnet-install.sh ./dotnet-install.sh

RUN chmod +x ./dotnet-install.sh \
    && ./dotnet-install.sh -v 9.0.303 --install-dir /usr/share/dotnet --no-path \
    && ./dotnet-install.sh -v 8.0.412 --install-dir /usr/share/dotnet --no-path \
    && rm dotnet-install.sh

ENV PATH="$PATH:/usr/share/dotnet"

# https://github.com/dotnet/runtime/issues/65874
RUN update-crypto-policies --set LEGACY

# Install dependencies
RUN dnf install -y \
    cmake-3.26.5-2.el9 \
    clang-20.1.3-1.el9 \
    git-2.43.5-1.el9

WORKDIR /project
