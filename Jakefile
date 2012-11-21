var 
    fs      = require('fs'),
    path    = require('path'),
    njake   = require('./src/njake'),
    msbuild = njake.msbuild,
    nuget   = njake.nuget,
    config  = {
        rootPath: __dirname,
        version: fs.readFileSync('VERSION', 'utf-8')
    };

console.log('SimpleOwin v' + config.version)

msbuild.setDefaults({
    properties: { Configuration: 'Release' },
    processor: 'x86',
    version: 'net4.0'
})

nuget.setDefaults({
    _exe: 'src/.nuget/NuGet.exe',
    verbose: true
})

task('default', ['build', 'nuget:pack'])

directory('dist/')
directory('working/')

desc('Build')
task('build', function () {
    msbuild({
        file: 'src/SimpleOwin.sln',
        targets: ['Build']
    })
}, { async: true })

desc('Clean all')
task('clean', function () {
    msbuild({
        file: 'src/SimpleOwin.sln',
        targets: ['Clean']
    }, function(code) {
        if (code !== 0) fail('msbuild failed')
        jake.rmRf('bin/')
        jake.rmRf('working/')
        jake.rmRf('dist/')
    })
}, { async: true })

namespace('nuget', function () {

    namespace('pack', function () {

        directory('dist/symbolsource/', ['dist/'])

        nugetNuspecs = fs.readdirSync('src/nuspec').filter(function (nuspec) {
            return nuspec.indexOf('.nuspec') > -1
        })

        symbolsourceNuspecs = fs.readdirSync('src/nuspec/symbolsource')

        npkgDeps = [
            'nuget:pack:SimpleOwinExtensions.cs.pp',
            'nuget:pack:SimpleOwinAspNetHost.cs.pp'
        ]

        nugetNuspecs.forEach(function (nuspec) {
            npkgDeps.push('nuget:pack:' + nuspec)
            task(nuspec, ['dist/', 'build'], function () {
                nuget.pack({
                    nuspec: 'src/nuspec/' + nuspec,
                    version: config.version,
                    outputDirectory: 'dist/'
                })
            })
        })

        symbolsourceNuspecs.forEach(function (nuspec) {
            npkgDeps.push('nuget:pack:symbolsource' + nuspec)
            task('symbolsource' + nuspec, ['dist/symbolsource/', 'build'], function () {
                nuget.pack({
                    nuspec: 'src/nuspec/symbolsource/' + nuspec,
                    version: config.version,
                    outputDirectory: 'dist/symbolsource/'
                })
            })
        })

        task('SimpleOwinExtensions.cs.pp', ['working/', 'dist/', 'build'], function () {
            console.log('Generating working/SimpleOwinExtensions.cs.pp');

            var csFile = fs
                .readFileSync('src/SimpleOwin.Extensions/SimpleOwinExtensions.cs', 'utf-8')
                .replace('// VERSION:', '// VERSION: ' + config.version)
                .replace('namespace SimpleOwin.Extensions', 'namespace $rootnamespace$')
                .replace('public static class', 'internal static class');

            fs.writeFileSync('working/SimpleOwinExtensions.cs.pp', csFile);
        })

        task('SimpleOwinAspNetHost.cs.pp', ['working/', 'dist/', 'build'], function () {
            console.log('Generating working/SimpleOwinAspNetHost.cs');

            var csFile = fs
                .readFileSync('src/SimpleOwin.Hosts.AspNet/SimpleOwinAspNetHost.cs', 'utf-8')
                .replace('// VERSION:', '// VERSION: ' + config.version)
                .replace('namespace SimpleOwin.Hosts.AspNet', 'namespace $rootnamespace$')
                .replace(/public class/g, 'internal class');
            fs.writeFileSync('working/SimpleOwinAspNetHost.cs.pp', csFile);
        })

    })

    desc('Create nuget packages')
    task('pack', npkgDeps)

})
